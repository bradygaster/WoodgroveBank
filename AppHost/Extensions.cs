using Aspire.Hosting.Azure;
using Azure.Provisioning;

namespace AppHost;

public static class Extensions
{
    public static ProvisioningParameter AsProvisioningParameter(this EndpointReference endpointReference, AzureResourceInfrastructure infrastructure, string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(endpointReference);
        ArgumentNullException.ThrowIfNull(infrastructure);

        parameterName ??= Infrastructure.NormalizeIdentifierName(((IManifestExpressionProvider)endpointReference).ValueExpression);

        infrastructure.AspireResource.Parameters[parameterName] = endpointReference;

        var parameter = infrastructure.GetResources().OfType<ProvisioningParameter>().FirstOrDefault(p => p.IdentifierName == parameterName);
        if (parameter is null)
        {
            parameter = new ProvisioningParameter(parameterName, typeof(string));
            infrastructure.Add(parameter);
        }

        return parameter;
    }

    public static ProvisioningParameter AsProvisioningParameter(this ReferenceExpression expression, AzureResourceInfrastructure infrastructure, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(infrastructure);

        infrastructure.AspireResource.Parameters[parameterName] = expression;

        var parameter = infrastructure.GetResources().OfType<ProvisioningParameter>().FirstOrDefault(p => p.IdentifierName == parameterName);
        if (parameter is null)
        {
            parameter = new ProvisioningParameter(parameterName, typeof(string));
            infrastructure.Add(parameter);
        }

        return parameter;
    }
}
