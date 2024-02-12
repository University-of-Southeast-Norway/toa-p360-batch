﻿using P360Client;

namespace DfoToa.Archive.Steps;

public abstract class Step : IStep
{
    private readonly string _name;

    public bool Success { get; set; }
    public Exception Exception { get; private set; }

    protected Step(string name)
    {
        _name = name;
    }


    public async Task Execute(Client client)
    {
        if (Success) return;
        try
        {
            await ExecuteStep(client);
        }
        catch (Exception ex)
        {
            Exception = ex;
            throw;
        }
        Success = true;
    }
    protected abstract Task ExecuteStep(Client client);


    public async Task Execute<TStep>(Client client, TStep fromStep) where TStep : Step
    {
        if (Success) return;
        try
        {
            await ExecuteStep(client, fromStep);
        }
        catch (Exception ex)
        {
            Exception = ex;
            throw;
        }
        Success = true;
    }
    protected abstract Task ExecuteStep<TStep>(Client client, TStep fromStep) where TStep : Step;
}
