# SessyController

This code aims to be a better solution for (dis)charging the Sessy batteries in the home.

To use this code you need to create an account and get a SecurityToken from ENTSO-E.

In the AppSettings.json the following must configured for your personal installation:

    {
        "Logging": {
            "LogLevel": {
                "Default": "Information",
                "Microsoft.AspNetCore": "Warning"
            }
        },
        "AllowedHosts": "*",

        "ENTSO-E:InDomain": "10YNL----------L", // EIC-code voor Nederland
        "ENTSO-E:ResolutionFormat": "PT60M",

        "Kestrel:Certificates:Development:Password": "Your self-signed certificate password for Swagger web front end", // Move to secrets.json
        "ENTSO-E:SecurityToken": "Your securityToken from ENTSO-E", // Move to secrets.json

        "Sessy:Batteries": {
            "Batteries": {
                "1": {
                    "Name": "Battery 1",
                    "UserId": "Dongle user id",     // Move to secrets.json
                    "Password": "Dongle password",  // Move to secrets.json
                    "BaseUrl": "http://192.168.1.xxx", // IP Address of your first battery
                    //"BaseUrl": "http://host.docker.internal:3001" // Mock server (Mockoon)
                },
                "2": {
                    "Name": "Battery 2",
                    "UserId": "Dongle user id",     // Move to secrets.json
                    "Password": "Dongle password",  // Move to secrets.json
                    "BaseUrl": "http://192.168.1.xxx", // IP Address of your second battery
                    // "BaseUrl": "http://host.docker.internal:3001" // Mock server (Mockoon)
                },
                "3": {
                    "Name": "Battery 3",
                    "UserId": "Dongle user id",     // Move to secrets.json
                    "Password": "Dongle password",  // Move to secrets.json
                    "BaseUrl": "http://192.168.1.xxx", // // IP Address of your third battery
                    // "BaseUrl": "http://host.docker.internal:3001" // Mock server (Mockoon)
                }
            }
        }
    }

If you want to join in on GitHub I advice you to put the 'UserId' and 'Password' in the secrets.json of your Visual Studio project.

Here's an example for the secrest.json:

    {
        "Sessy:Batteries": {
            "Batteries": {
                "1": {
                    "UserId": "Dongle user id",
                    "Password": "Dongle password"
                },
                "2": {
                    "UserId": "Dongle user id",
                    "Password": "Dongle password"
                },
                "3": {
                    "UserId": "Dongle user id",
                    "Password": "Dongle password"
                }
            }
        },
        "Kestrel:Certificates:Development:Password": "Your self-signed certificate password for Swagger web front end",
        "ENTSO-E:SecurityToken": "Your securityToken from ENTSO-E"
    }