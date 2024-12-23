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

        "Sessy:Batteries": {
            "Batteries": {
                "1": {
                    "Name": "Battery 1",
                    "UserId": "Dongle user id",
                    "Password": "Dongle password"
                    "BaseUrl": "http://192.168.1.xxx", // IP Address of your first battery
                    //"BaseUrl": "http://host.docker.internal:3001" // Mock server (Mockoon)
                },
                "2": {
                    "Name": "Battery 2",
                    "UserId": "Dongle user id",
                    "Password": "Dongle password"
                    "BaseUrl": "http://192.168.1.xxx", // IP Address of your second battery
                    // "BaseUrl": "http://host.docker.internal:3001" // Mock server (Mockoon)
                },
                "3": {
                    "Name": "Battery 3",
                    "UserId": "Dongle user id",
                    "Password": "Dongle password"
                    "BaseUrl": "http://192.168.1.xxx", // // IP Address of your third battery
                    // "BaseUrl": "http://host.docker.internal:3001" // Mock server (Mockoon)
                }
            }
        }
    }

If you want to join in on GitHub I advice you to put the 'UserId' and 'Password' in the secrets.json of your Visual Studio project.