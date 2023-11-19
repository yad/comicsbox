# This guide is for Raspberry Pi 2, ARM32

## Install

`sudo apt install nodejs npm git`

dotnet, download https://dotnet.microsoft.com/en-us/download/dotnet/7.0

`wget https://download.visualstudio.microsoft.com/download/pr/6d070d9e-6748-49b4-8ebb-cfd74c25b89c/530fb7c5244e9a1ac798820335c58c35/dotnet-sdk-7.0.404-linux-arm.tar.gz`

`sudo mkdir -p /opt/dotnet`

`sudo tar -xf dotnet-sdk-7.0.404-linux-arm.tar.gz -C /opt/dotnet`

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

`sudo systemctl restart comicsbox.service`

`sudo systemctl status comicsbox.service`