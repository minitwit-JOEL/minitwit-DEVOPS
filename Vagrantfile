# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
    ## Syncs files form the host to the guest vm mounting point (but not the other way)
    config.vm.synced_folder ".", "/vagrant", type: "rsync"
    config.vm.box = "generic/ubuntu2204"
  
    ## Database server
    config.vm.define "db", primary:true do |server|
      ## Defining db server on the private network, disallows access from the outside.
      server.vm.network "private_network", ip: "192.168.20.2"
  
      server.vm.provider "minitwit-db" do |vb|
        ## Is only on a gut feeling, we should check with the limitations of digigtalocean
        vb.memory = "1024"
      end
      server.vm.hostname = "minitwit-db"
      ## TODO: Setup of database:
      server.vm.provision "shell", privileged: false, inline: <<-SHELL
        echo "test"
        SHELL
    end
  
    ## Api server
    config.vm.define "minitwit-api", pirmary: true do |server|
      ## TODO: Change to public network, and integrate to work with digitalocean in order to expose to public
      server.vm.network "private_network", ip: "192.168.20.3"
      server.vm.provider "virtualbox" do |vb|
        ## Is only on a gut feeling, we should check with the limitations of digigtalocean
        vb.memory = "1024"
      end
      server.vm.hostname = "minitwit-api"
      ## TODO: Setup of api, have not been tested
      server.vm.provision "shell", privileged: false, inline: <<-SHELL
        sudo apt-get update && \
          sudo apt-get install -y dotnet-sdk-9.0
  
        dotnet publish "minitwit.Api/minitwit.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
        dotnet /app/publish/minitwit.Api.dll
      SHELL
    end
  
    ## Web server
    config.vm.define "minitwit-server", primary: true do |server|
      ## TODO: Change to public network, and integrate to work with digitalocean in order to expose to public
      server.vm.network "private_network", ip: "192.168.20.4"
      server.vm.provider "virtualbox" do |vb|
        ## Is only on a gut feeling, we should check with the limitations of digigtalocean
        vb.memory = "1024"
      end
      server.vm.hostname = "minitwit-server"
      ## TODO: Setup of database have not been test
      server.vm.provision "shell", privileged: false, inline: <<-SHELL
        sudo apt install npm
        npm install
        npm run build
        npm run start
      SHELL
    end
  end
  