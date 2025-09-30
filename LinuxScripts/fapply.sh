#!/bin/bash
#!/bin/bash
file="/usr/local/sam530/publish/Sam530.dll"
if [ ! -f "$file" ]
then
	echo "$0: File '${file}' not found."
else
    cd /usr/local/sam530/
    sudo cp -r /usr/local/sam530/publish/* ./service 
    sudo cp -r /usr/local/sam530/publish/selba/* . 
    sudo rm -r /usr/local/sam530/publish
    sync
    echo "Firmware Updated"
fi



