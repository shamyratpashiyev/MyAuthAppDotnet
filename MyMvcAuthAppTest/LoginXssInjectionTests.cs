using System.Net;
using System.Text;
using Newtonsoft.Json;
using Shouldly;

namespace MyMvcAuthAppTest;

public class LoginXssInjectionTests
{
    private readonly HttpClient _client;
    
    public LoginXssInjectionTests()
    {
        _client = new HttpClient();
    }
    
    
    [Theory]
    [InlineData("<script>alert('xss')</script>", false)]
    [InlineData("<img src=x onerror=alert(1)>", false)]
    [InlineData("testUser<script>alert('xss')</script>", true)] //It should automatically remove xss part
    public async Task LoginEndpoint_ShouldNotAllowXss(string username,  bool correctUsername)
    {
        // Arrange
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(username), "UserName");
        formData.Add(new StringContent("PASSword123$%"), "Password");
        formData.Add(new StringContent("false"), "RememberMe");
        
        // Act
        var response = await _client.PostAsync(GlobalVariables.BackendBaseUrl + "/Account/Login", formData);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        if (correctUsername)
        {
            stringResponse.ShouldNotContain("Invalid login attempt");    
        }
        else
        {
            stringResponse.ShouldContain("Invalid login attempt");
        }
    }
}