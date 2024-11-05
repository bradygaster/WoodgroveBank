namespace WoodgroveBank.Abstractions;

public interface IAccountTransactionGrain : IGrainWithGuidKey          
{
    Task<AccountTransaction> GetTransaction();
    Task SetTransaction(AccountTransaction transaction);
    Task Process();
}
