## Vagrantfile for launching a VM for the api and web-server

Vagrant.configure("2") do |config|
  config.vm.box = 'digital_ocean'
  config.vm.synced_folder ".", "/vagrant", disabled: true
  config.vm.provision "file", source: "deploy.sh", destination: "/home/github/deploy.sh"
  config.vm.provision "file", source: ".secrets-production", destination: "/home/github/.secrets-production"
  config.vm.provision "file", source: "prometheus/prometheus.yml", destination: "/home/github/prometheus/prometheus.yml"
  config.vm.provision "file", source: "grafana/dashboards/work_in_progress_dashboard.json", destination: "/home/github/grafana/dashboards/work_in_progress_dashboard.json"
  config.vm.provision "file", source: "grafana/provisioning/dashboards/dashboard.yaml", destination: "/home/github/grafana/provisioning/dashboards/dashboard.yaml"
  config.vm.provision "file", source: "grafana/provisioning/datasources/datasource.yaml", destination: "/home/github/grafana/provisioning/datasources/datasource.yaml"
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
      echo "Installing docker"

      # Wait for APT and DPKG locks to be released
      while sudo fuser /var/lib/apt/lists/lock >/dev/null 2>&1 || \
        sudo fuser /var/lib/dpkg/lock-frontend >/dev/null 2>&1; do
        echo "Waiting for apt/dpkg lock to be released..."
        sleep 2
      done

      echo "APT/DPKG locks are released"

      # Following official docker installation: https://docs.docker.com/engine/install/ubuntu/

      # Add Docker's official GPG key:
      sudo apt-get update
      sudo apt-get install -y ca-certificates curl
      sudo install -m 0755 -d /etc/apt/keyrings
      sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
      sudo chmod a+r /etc/apt/keyrings/docker.asc

      # Wait for APT and DPKG locks to be released
      while sudo fuser /var/lib/apt/lists/lock >/dev/null 2>&1 || \
        sudo fuser /var/lib/dpkg/lock-frontend >/dev/null 2>&1; do
        echo "Waiting for apt/dpkg lock to be released..."
        sleep 2
      done

      echo "APT/DPKG locks are released"

      # Add the repository to Apt sources:
      echo \
        "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
        $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}") stable" | \
        sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

      # Wait for APT and DPKG locks to be released
      while sudo fuser /var/lib/apt/lists/lock >/dev/null 2>&1 || \
        sudo fuser /var/lib/dpkg/lock-frontend >/dev/null 2>&1; do
        echo "Waiting for apt/dpkg lock to be released..."
        sleep 2
      done

      echo "APT/DPKG locks are released"
      
      sudo apt-get update
      sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
    SHELL

    # Add droplet to firewall through doctl
    server.vm.provision "shell", env: { "DIGITAL_OCEAN_TOKEN" => ENV["DIGITAL_OCEAN_TOKEN"] }, inline: <<-SHELL
      echo "Configuring firewall"

      curl -sL https://github.com/digitalocean/doctl/releases/download/v1.105.0/doctl-1.105.0-linux-amd64.tar.gz | tar -xz
      mv doctl /usr/local/bin

      # Authenticate doctl
      doctl auth init -t "$DIGITAL_OCEAN_TOKEN"

      # Get this droplet's ID using metadata
      DROPLET_ID=$(curl -s http://169.254.169.254/metadata/v1/id)
      DROPLET_IP=$(curl -s http://169.254.169.254/metadata/v1/interfaces/public/0/ipv4/address)

      echo "This droplets IP addess it $DROPLET_IP"

      # Get the firewall id by name
      FIREWALL_NAME="fireball"
      FIREWALL_ID=$(doctl compute firewall list --format ID,Name --no-header | grep "\\b$FIREWALL_NAME\\b" | awk '{print $1}')
      
      echo "Adding droplet to firewall: $DROPLET_ID -> $FIREWALL_ID"

      # Add this droplet to the existing firewall
      doctl compute firewall add-droplets "$FIREWALL_ID" --droplet-ids "$DROPLET_ID" 
      doctl compute firewall add-rules "$FIREWALL_ID" --inbound-rules "protocol:tcp,ports:5432,address:$DROPLET_IP/32"
    SHELL

    # Run deploy.sh
    server.vm.provision "shell", inline: <<-SHELL
      echo "Creating github user and executing deploy.sh"

      # Create github user and folder
      sudo useradd -m github -g docker

      # Make deploy.sh executable
      chmod +x /home/github/deploy.sh
      
      # Add the github public keys to ssh file
      echo "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIPcpMAMTqI/SdcR7dW64FzDYurU+NVezpt2/PBOxwT8z github@minitwit-joel" >> /home/github/.ssh/autorized_keys

      # Execute deploy.sh script
      cd /home/github/
      /home/github/deploy.sh
    SHELL
  end
end