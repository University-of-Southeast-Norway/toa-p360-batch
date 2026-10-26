// See https://aka.ms/new-console-template for more information
using Refit;
using Service;

internal interface ISupportServiceDirectApi
{
    [Post("/Biz/v2/api/call/SI.Data.RPC/SI.Data.RPC/SupportService/Ping")]
    Task<ApiResponse<string>> Ping([Query] AuthKeyQuery authKeyQuery);

    [Post("/Biz/v2/api/call/SI.Data.RPC/SI.Data.RPC/SupportService/GetCodeTableRows")]
    Task<ApiResponse<P360Client.DTO.GetCodeTableRowsResponse>> GetCodeTableRows([Query] AuthKeyQuery authKeyQuery, [Body] P360Client.DTO.GetCodeTableRowsArgs args);
}