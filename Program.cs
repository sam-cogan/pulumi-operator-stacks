using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using System.Collections.Generic;

return await Pulumi.Deployment.RunAsync(() =>
{
   var accessToken = new Secret("deploymentCreds", new SecretArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Namespace = "pulumi-operator",
                Name = "pulumi-deployment-creds"
            },
            StringData =
            {
                {"accessToken", config.RequireSecret("pulumiAccessToken")},
                {"azureAppId", config.Require("azureAppId")},
                {"azurePassword", config.RequireSecret("azurePassword")},
                {"azureTenant", config.Require("azureTenant")},

            }
        });

      var accessTokenSecret = new Secret("accessTokenSecret", new SecretArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Namespace = "pulumi-operator",
                Name = "access-token-secret"
            },
            StringData =
            {
                {"accessToken", config.RequireSecret("pulumiAccessToken")},

            }
        });

        var deploymentCredsEnv = new Secret("deploymentCredsEnv", new SecretArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Namespace = "pulumi-operator",
                Name = "pulumi-deployment-creds-env"
            },
            StringData =
            {

                {"ARM_CLIENT_ID", config.Require("azureAppId")},
                {"ARM_CLIENT_SECRET", config.RequireSecret("azurePassword")},
                {"ARM_TENANT_ID", config.Require("azureTenant")},
                {"ARM_SUBSCRIPTION_ID", config.Require("azureSubscription")},

            }
        });

        var myStack = new Pulumi.Kubernetes.ApiExtensions.CustomResource("myStack", new StackArgs
        {

            Metadata = new ObjectMetaArgs
            {
                Namespace = "pulumi-operator",
                Name = "op-demo-pulumi"
            },
            Spec = new StackSpecArgs
            {

                Stack = "samcogan/operator-demo/op-demo-pulumi",
                ProjectRepo = "https://github.com/sam-cogan/pulumi-operator-demo",
                Branch = "refs/heads/main",
                DestroyOnFinalize = true,
                AccessTokenSecret = accessTokenSecret.Metadata.Apply(md => md.Name),
                EnvSecrets = new InputList<string>(){deploymentCredsEnv.Metadata.Apply(md => md.Name)}
            }
        });
});