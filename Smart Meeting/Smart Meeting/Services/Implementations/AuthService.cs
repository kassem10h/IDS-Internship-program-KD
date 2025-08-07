using Microsoft.AspNetCore.Identity;
using SmartMeeting.DTOs;
using SmartMeeting.Helpers;
using SmartMeeting.Models;
using SmartMeeting.Repositories.Interfaces;
using SmartMeeting.Services.Interfaces;
using System.Security.Cryptography;

namespace SmartMeeting.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenGenerator jwtTokenGenerator,
            IGenericRepository<RefreshToken> refreshTokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        Errors = new List<string> { "Email is already registered" }
                    };
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    EmailConfirmed = true // For simplicity, auto-confirm emails
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Failed to create user",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Add user to default role
                await _userManager.AddToRoleAsync(user, RoleConstants.User);

                // Generate tokens
                var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
                var refreshToken = GenerateRefreshToken(ipAddress);

                // Save refresh token
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                var userRoles = await _userManager.GetRolesAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "User registered successfully",
                    AccessToken = accessToken.Token,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = accessToken.ExpiresAt,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Roles = userRoles.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null || !user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "Authentication failed" }
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    var message = result.IsLockedOut ? "Account is locked out" : "Invalid email or password";
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = message,
                        Errors = new List<string> { "Authentication failed" }
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
                var refreshToken = GenerateRefreshToken(ipAddress);

                // Revoke old refresh tokens and add new one
                await RevokeOldRefreshTokensAsync(user);
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                var userRoles = await _userManager.GetRolesAsync(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken.Token,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = accessToken.ExpiresAt,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Roles = userRoles.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var token = await _refreshTokenRepository.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
                if (token == null || !token.IsActive)
                {
                    return new RefreshTokenResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token",
                        Errors = new List<string> { "Token validation failed" }
                    };
                }

                var user = await _userManager.FindByIdAsync(token.UserId);
                if (user == null || !user.IsActive)
                {
                    return new RefreshTokenResponseDto
                    {
                        Success = false,
                        Message = "User not found or inactive",
                        Errors = new List<string> { "User validation failed" }
                    };
                }

                // Revoke old token
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
                token.IsRevoked = true;

                // Generate new tokens
                var newAccessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
                var newRefreshToken = GenerateRefreshToken(ipAddress);

                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
                await _refreshTokenRepository.SaveChangesAsync();

                return new RefreshTokenResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken.Token,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresAt = newAccessToken.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                return new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = "An error occurred during token refresh",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var token = await _refreshTokenRepository.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
                if (token == null || !token.IsActive)
                    return false;

                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;
                token.IsRevoked = true;

                await _refreshTokenRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                // Revoke all refresh tokens
                var activeTokens = user.RefreshTokens.Where(rt => rt.IsActive).ToList();
                foreach (var token in activeTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.IsRevoked = true;
                }

                await _userManager.UpdateAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                return result.Succeeded;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserInfoDto?> GetUserInfoAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return null;

                var roles = await _userManager.GetRolesAsync(user);

                return new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Roles = roles.ToList()
                };
            }
            catch
            {
                return null;
            }
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            };
        }

        private async Task RevokeOldRefreshTokensAsync(ApplicationUser user)
        {
            var oldTokens = user.RefreshTokens.Where(rt => rt.IsActive && rt.ExpiryDate < DateTime.UtcNow.AddDays(-1)).ToList();
            foreach (var token in oldTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.IsRevoked = true;
            }
        }
    }
}
