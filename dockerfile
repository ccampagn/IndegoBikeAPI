FROM jenkins/jenkins:lts-jdk17

USER root

# 1. Install EVERYTHING needed: libicu for .NET runtime, plus download tools
RUN apt-get update && apt-get install -y \
    libicu-dev \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# 2. Install .NET 10 using the official script
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --install-dir /usr/share/dotnet

# 3. Setup paths and symlinks
RUN ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# 4. Global Environment Variables
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="${PATH}:${DOTNET_ROOT}"
# Optional: Set this to 1 if you want to bypass ICU entirely (not recommended for most apps)
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0 

# 5. Permissions
RUN chown -R jenkins:jenkins /usr/share/dotnet

USER jenkins