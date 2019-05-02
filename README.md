# Update_AppImage
C# application to upload or update the application logo in Azure AD

This is a sample application showing how to use c# to upload or update an application logo in Azure AD.
This app requires you to create an app registration in AzureAD with AAD Graph permissions and a client secret.
You will plug those setting values into the AuthSettings.cs file.

You will need the Application Id to plug into the client_id property, the client_secret and your tenant_id.

The Program.cs file will need the AppId for the application registration in your tenant that you want to update the logo with and also the root of the file path on your computer (where the application runs from -- most likey, the debug bin folder).

Please note, the logo image must conform to the parameters for size, type, etc. as indicated in the portal should you manually upload a logo.
