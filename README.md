# This guide is for Raspberry Pi 2, ARM32

## Install

`sudo apt install nodejs npm git`

dotnet, download https://dotnet.microsoft.com/en-us/download/dotnet/8.0

`wget https://download.visualstudio.microsoft.com/download/pr/7ec1a911-afeb-47fa-a1d0-fa22cd980b32/157c20841cbf1811dd2a7a51bf4aaf88/dotnet-sdk-8.0.100-linux-arm.tar.gz`

`sudo rm -rf /opt/dotnet`

`sudo mkdir -p /opt/dotnet`

`sudo tar -xf dotnet-sdk-8.0.100-linux-arm.tar.gz -C /opt/dotnet`

`sudo ln -s /opt/dotnet/dotnet /usr/bin`

`dotnet --version`

# Build

`useradd --system --no-create-home comicsbox`

`sudo mkdir -p /opt/comicsbox`

`sudo chown -R comicsbox:comicsbox /opt/comicsbox/`

`sudo dotnet publish --runtime linux-arm --self-contained -o /opt/comicsbox`

# Register service

`sudo cp comicsbox.service /etc/systemd/system/comicsbox.service`

`sudo chown root:root /etc/systemd/system/comicsbox.service`

`sudo systemctl enable comicsbox.service`

`sudo systemctl start comicsbox.service`

`sudo systemctl status comicsbox.service`

# Update

`git pull`

`sudo dotnet publish --runtime linux-arm --self-contained -o /opt/comicsbox`

`sudo chown -R comicsbox:comicsbox /opt/comicsbox/`

`sudo systemctl restart comicsbox.service`

`sudo systemctl status comicsbox.service`