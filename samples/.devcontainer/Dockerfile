FROM ubuntu:22.04

RUN apt update &&\
    apt upgrade -y &&\
    apt install -y curl wget git zsh libicu70 bash-completion\
                   libglib2.0-0 libnss3 libnspr4 libdbus-1-3\
                   libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2\
                   libatspi2.0-0 libxcomposite1 libxdamage1\
                   libxfixes3 libxrandr2 libgbm1 libxkbcommon0\
                   libpango-1.0-0 libcairo2 libasound2 gnupg2

# Install Azure CLI
RUN curl -o az-cli.sh -L https://aka.ms/InstallAzureCLIDeb &&\
    chmod +x az-cli.sh &&\
    ./az-cli.sh -y

ARG user=vscode

RUN useradd -ms /bin/bash $user

RUN mkdir /samples && chown $user:$user /samples

USER $user
WORKDIR /home/$user

# Add Node
ARG node_version
RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
ENV NVM_DIR /home/$user/.nvm
RUN . "$NVM_DIR/nvm.sh" &&\
    nvm install v$node_version &&\
    nvm use v$node_version

# Add .NET
ARG dotnet_version
RUN curl -o dotnet-install.sh -L https://dot.net/v1/dotnet-install.sh &&\
    chmod +x dotnet-install.sh &&\
    ./dotnet-install.sh --install-dir /home/$user/.dotnet --version $dotnet_version

RUN echo "export DOTNET_ROOT=\"/home/${user}/.dotnet\"" >> /home/$user/.bashrc &&\
    echo "export PATH=\"\$PATH:\$DOTNET_ROOT:\$DOTNET_ROOT/tools\"" >> /home/$user/.bashrc

# Configure shell
RUN curl -L https://raw.github.com/git/git/master/contrib/completion/git-prompt.sh -o /home/$user/.git-prompt.sh &&\
    echo "source /home/$user/.git-prompt.sh" >> /home/$user/.bashrc &&\
    echo "export PS1='\[\033[01;32m\]\u\[\033[00m\]:\[\033[01;34m\]\w\[\033[00m\]\[\033[01;33m\]\$(__git_ps1)\[\033[00m\]\$ '" >> /home/$user/.bashrc
