using Castle.Components.DictionaryAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VendTech.Areas.Api.Controllers.b2b;
using VendTech.BLL.Interfaces;
using VendTech.BLL.PlatformApi;

namespace Vendtech.Test
{
    //[TestClass]
    //public class BsbUsersEndpointsTest
    //{
    //    private HttpClient _client;
    //    private Mock<ICalculatorService> _mockCalService;
    //    private Mock<IErrorLogManager> _mockLoggerService;
    //    private b2btestController _controller; 

    //    [TestInitialize]
    //    public void Setup()
    //    {
    //        _mockCalService = new Mock<ICalculatorService>();
    //        _mockLoggerService = new Mock<IErrorLogManager>();
    //        _controller = new b2btestController(_mockCalService.Object, _mockLoggerService.Object);

    //        var handler = new HttpClientHandler { AllowAutoRedirect = false };
    //        _client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:56549/api/b2btest") };
    //        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //    }

    //    [TestMethod]
    //    public async Task Create_user_credential()
    //    {
    //        //Arrange
    //        var result = 2;
    //        // result can also be an object
    //        _mockCalService.Setup(x => x.Add(1, 1)).Returns(result);
    //        HttpResponseMessage response = null;
            
    //        // Act
    //        response = await _client.GetAsync("/get");
    //        if (response.StatusCode == HttpStatusCode.Redirect)
    //        {
    //            var redirectUri = response.Headers.Location;
    //            Assert.Fail($"Request was redirected to {redirectUri}");
    //        }

    //        // Assert
    //        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    //        var returned = await response.Content.ReadAsStringAsync();
    //        Assert.AreEqual("2", returned);
    //    }
    //}
}
