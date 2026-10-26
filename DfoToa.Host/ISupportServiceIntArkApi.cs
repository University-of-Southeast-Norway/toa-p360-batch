// See https://aka.ms/new-console-template for more information
using Refit;

internal interface ISupportServiceIntArkApi
{
    [Post("/Ping")]
    Task<ApiResponse<string>> Ping();

    [Post("/GetCodeTableRows")]
    Task<ApiResponse<P360Client.DTO.GetCodeTableRowsResponse>> GetCodeTableRows([Body] P360Client.DTO.GetCodeTableRowsArgs args);
}
