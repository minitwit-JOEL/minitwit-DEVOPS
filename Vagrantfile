## Vagrantfile for launching a VM for the api and web-server

Vagrant.configure("2") do |config|
  config.vm.box = 'digital_ocean'
  config.vm.synced_folder ".", "/vagrant", disabled: true
  config.vm.provision "file", source: "deploy.sh", destination: "/home/vagrant/deploy.sh"
  config.vm.provision "file", source: ".secrets-production", destination: "/home/vagrant/.secrets-production"
  config.ssh.private_key_path = '~/.ssh/id_rsa'
  config.ssh.insert_key = false

  config.vm.define "web-droplet-0" do |server|
    config.vm.provider :digital_ocean do |provider, override|
      provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
      provider.image = 'ubuntu-22-04-x64'
      provider.region = 'fra1'
      provider.size = 's-1vcpu-2gb-amd'
      provider.backups_enabled = false
      provider.private_networking = false
      provider.ipv6 = false
      provider.monitoring = true
      provider.ssh_key_name = "lukas-wsl"
    end

    server.vm.hostname = "web-droplet-0"

    server.vm.provision "shell", inline: <<-SHELL

      # Wait for any other apt process to finish
      while fuser /var/lib/apt/lists/lock >/dev/null 2>&1; do
        echo "Waiting for apt lock to be released..."
        sleep 1
      done

      echo "apt lock is released"

      # Following official docker installation: https://docs.docker.com/engine/install/ubuntu/

      # Add Docker's official GPG key:
      sudo apt-get update
      sudo apt-get install ca-certificates curl
      sudo install -m 0755 -d /etc/apt/keyrings
      sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
      sudo chmod a+r /etc/apt/keyrings/docker.asc

      # Add the repository to Apt sources:
      echo \
        "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
        $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}") stable" | \
        sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
      sudo apt-get update

      sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
    SHELL

    # Run deploy.sh
    server.vm.provision "shell", inline: <<-SHELL
      chmod +x /home/vagrant/deploy.sh
      /home/vagrant/deploy.sh
    SHELL
  end
end