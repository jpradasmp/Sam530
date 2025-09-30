#!/bin/bash
#!/bin/bash
#!/bin/bash
file="/usr/local/sam530/service/Uploads/Sam530.gz"
if [ ! -f "$file" ]
then
	echo "$0: File '${file}' not found."
else
	cd /usr/local/sam530/
	sudo cp /usr/local/sam530/service/Uploads/Sam530.gz .
	sudo rm -r /usr/local/sam530/service/Uploads/*
	sudo rm -r /usr/local/sam530/publish
	sudo tar -xvzf /usr/local/sam530/Sam530.gz
	sudo rm  /usr/local/sam530/Sam530.gz
	sync
fi
