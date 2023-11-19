
#!/bin/sh
set -e

git pull

sudo dotnet publish --runtime linux-arm --self-contained -o /opt/comicsbox

sudo chown -R comicsbox:comicsbox /opt/comicsbox/

sudo systemctl restart comicsbox.service

sudo systemctl status comicsbox.service