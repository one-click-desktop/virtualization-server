FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base

RUN apt-get update && apt-get install libvirt-dev build-essential ruby-dev curl python-dev python3-pip openssh-client ovmf -y
#https://4sysops.com/archives/how-to-deploy-ansible-inside-a-docker-container/#Content-bal-title
RUN pip3 install --upgrade pip && \
  pip3 install --upgrade virtualenv && \
  pip3 install pywinrm && \
  pip3 install ansible
RUN curl https://releases.hashicorp.com/vagrant/2.2.19/vagrant_2.2.19_x86_64.deb -o vagrant_2.2.19_x86_64.deb && dpkg -i vagrant_2.2.19_x86_64.deb

ENV GEM_HOME=/root/.vagrant.d/gems/2.7.4/
ENV GEM_PATH=/root/.vagrant.d/gems/2.7.4/
ENV PATH=/opt/vagrant/embedded/bin/:$PATH

RUN curl -L --max-redirs 5 https://github.com/vagrant-libvirt/vagrant-libvirt/files/7885369/prerelease-gems-for-libvirt-nvram-support.zip\
 -o prerelease-gems-for-libvirt-nvram-support.zip

# https://github.com/vagrant-libvirt/vagrant-libvirt/issues/1371#issuecomment-1014980135
# NOTE:  Only the vagrant-libvirt gem was configured to add `.pre`-release suffix to version!
#  Be aware this can cause issues when it comes time to upgrade gems again
#  Also, this isn't technically supported by developers of vagrant or vagrant-libvirt
#  I'd advise avoiding this if you can wait for the new official gems to be released, or if you don't know what you're doing with installing ruby gems.
#  Use at your own risk!
RUN unzip prerelease-gems-for-libvirt-nvram-support.zip
WORKDIR prerelease-gems-for-libvirt-nvram-support/
RUN gem install ruby-libvirt-0.8.0.gem
RUN gem install fog-libvirt-0.9.0.gem
RUN gem install vagrant-libvirt-0.7.1.pre.27.gem
RUN gem vagrant plugin install vagrant-libvirt-0.7.1.pre.27.gem

ENTRYPOINT []