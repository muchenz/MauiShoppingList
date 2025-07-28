using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test_MauiApp1.Models;
using Test_MauiApp1.Models.Requests;
using Test_MauiApp1.Models.Response;

namespace Test_MauiApp1.Services
{
    public class UserService
    {


        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly StateService _stateService;

        public UserService(HttpClient httpClient, IConfiguration configuration, StateService stateService)
        {
            _httpClient = httpClient;

            //----------------------
            // HttpClientHandler handler = new HttpClientHandler();
            // handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;


            //  _httpClient = new HttpClient(handler);

            // var baseAddress = configuration.GetSection("AppSettings")["ShoppingWebAPIBaseAddress"];
            // _httpClient.BaseAddress = new Uri(baseAddress);

            // //_httpClient.BaseAddress = new Uri("https://192.168.8.222:5003/api/");
            //// _httpClient.BaseAddress = new Uri("https://94.251.148.187:5003/api/");

            // _httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");
            //----------------------
            _configuration = configuration;
            _stateService = stateService;
        }



        public async Task<MessageAndStatusAndData<UserNameAndTokensResponse>> GetTokenFromFacebookAccessToken(string accessFacebookToken)
        {
            var querry = new QueryBuilder();
            querry.Add("access_token", accessFacebookToken);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "User/FacebookToken" + await querry.GetQuerryUrlAsync());

            MessageAndStatusAndData<UserNameAndTokensResponse> message = null;
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                var data = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    var tokenData = JsonConvert.DeserializeObject<UserNameAndTokensResponse>(data);

                    return MessageAndStatusAndData<UserNameAndTokensResponse>.Ok(tokenData);
                }
                message = MessageAndStatusAndData<UserNameAndTokensResponse>.Fail(
                    JsonConvert.DeserializeObject<ProblemDetails>(data).Title);
            }
            catch
            {
                message = MessageAndStatusAndData<UserNameAndTokensResponse>.Fail("Connection problem.");
            }
            return message;
        }

        public async Task<MessageAndStatusAndData<UserNameAndTokensResponse>> LoginAsync(string userName, string password)
        {

            var loginRequest = new LoginRequest
            {
                UserName = userName,
                Password = password
            };

            var json = JsonConvert.SerializeObject(loginRequest);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/Login")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var source = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    source.Cancel();
                });


                var response = await _httpClient.SendAsync(requestMessage, source.Token);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return MessageAndStatusAndData<UserNameAndTokensResponse>.Fail("Invalid username or password.");
                }
                if (!response.IsSuccessStatusCode)
                {
                    return MessageAndStatusAndData<UserNameAndTokensResponse>.Fail("Some errors occured.");
                }

                var data = await response.Content.ReadAsStringAsync();

                var tokenAndUsername = JsonConvert.DeserializeObject<UserNameAndTokensResponse>(data);
                return MessageAndStatusAndData<UserNameAndTokensResponse>.Ok(tokenAndUsername);
            }
            catch
            {
                return MessageAndStatusAndData<UserNameAndTokensResponse>.Fail("Connection problem.");
            }


        }

        public async Task<User> GetUserDataTreeAsync()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "User/UserDataTree");

            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();

            var user = JsonConvert.DeserializeObject<User>(data);

            return user;
        }

        public async Task<MessageAndStatusAndData<string>> RegisterAsync(RegistrationModel model)
        {

            var loginRequest = new RegistrationRequest
            {
                UserName = model.UserName,
                Password = model.Password
            };

            var json = JsonConvert.SerializeObject(loginRequest);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "User/Register")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };


            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                return MessageAndStatusAndData<string>.Ok(token);
            }

            return response switch
            {
                { StatusCode: System.Net.HttpStatusCode.Conflict } =>
                     MessageAndStatusAndData<string>.Fail("User exists."),
                _ =>
                    MessageAndStatusAndData<string>.Fail("Server error."),
            };

        }

        public async Task<List<Invitation>> GetInvitationsListAsync()
        {

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "Invitation/InvitationsList");



            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();


            var invitations = JsonConvert.DeserializeObject<List<Invitation>>(data);


            return await Task.FromResult(invitations);
        }

        public async Task<MessageAndStatus> AcceptInvitationAsync(Invitation invitation)
        {
            return await UniversalInvitationAction(invitation, "AcceptInvitation");

        }
        public async Task<MessageAndStatus> RejectInvitaionAsync(Invitation invitation)
        {

            return await UniversalInvitationAction(invitation, "RejectInvitaion");

        }

        async Task<MessageAndStatus> UniversalInvitationAction(Invitation invitation, string actionName)
        {
            string serialized = JsonConvert.SerializeObject(invitation);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Invitation/" + actionName);


            requestMessage.Content = new StringContent(serialized);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                return MessageAndStatus.Ok();
            }
            return MessageAndStatus.Fail();
        }

        public async Task<List<ListAggregationWithUsersPermission>> GetListAggrWithUsersPermAsync()
        {

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "Permissions/ListAggregationWithUsersPermission");


            var response = await _httpClient.SendAsync(requestMessage);

            var data = await response.Content.ReadAsStringAsync();

            var lists = JsonConvert.DeserializeObject<List<ListAggregationWithUsersPermission>>(data);

            return lists;
        }

        public async Task<MessageAndStatus> AddUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {
            return await UniversalUserPermission(userPermissionToList, listAggregationId, "AddUserPermission");
        }

        public async Task<MessageAndStatus> ChangeUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {

            return await UniversalUserPermission(userPermissionToList, listAggregationId, "ChangeUserPermission");

        }


        public async Task<MessageAndStatus> DeleteUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {
            return await UniversalUserPermission(userPermissionToList, listAggregationId, "DeleteUserPermission");
        }

        public async Task<MessageAndStatus> InviteUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId)
        {
            return await UniversalUserPermission(userPermissionToList, listAggregationId, "InviteUserPermission");
        }

        private async Task<MessageAndStatus> UniversalUserPermission(UserPermissionToListAggregation userPermissionToList, int listAggregationId,
            string actionName)
        {
            var querry = new QueryBuilder();

            querry.Add("listAggregationId", listAggregationId.ToString());

            //var httpMethod = actionName == "DeleteUserPermission" ? HttpMethod.Delete : HttpMethod.Post;

            string serializedUser = JsonConvert.SerializeObject(userPermissionToList);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "Permissions/" + actionName + await querry.GetQuerryUrlAsync());


            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
              = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            SetRequestAuthorizationLevelHeader(requestMessage, listAggregationId);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                var problem = JsonConvert.DeserializeObject<ProblemDetails>(responseBody);

                return MessageAndStatus.Fail(problem.Title);
            }

            var message =  actionName switch
            {
                "AddUserPermission" => "User was added.",
                "ChangeUserPermission" => "Permission has changed.",
                "InviteUserPermission" => "Ivitation was added.",
                "DeleteUserPermission" => "User permission was deleted.",
                _ => throw new ArgumentException("Bad action name.")
            };

            return MessageAndStatus.Ok(message);
        }
        public async Task<bool> VerifyToken()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "User/VerifyToken2");

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        void SetRequestAuthorizationLevelHeader(HttpRequestMessage httpRequestMessage, int listAggregationId)
        {
            var token = _stateService.StateInfo.Token;

            if (token != null)
            {
                httpRequestMessage.Headers.Add("listAggregationId", listAggregationId.ToString());

                using (SHA256 mySHA256 = SHA256.Create())
                {

                    var bytes = Encoding.ASCII.GetBytes(token + listAggregationId.ToString());

                    var hashBytes = mySHA256.ComputeHash(bytes);

                    var hashString = Convert.ToBase64String(hashBytes);

                    httpRequestMessage.Headers.Add("Hash", hashString);
                }
            }


        }
    }
}
