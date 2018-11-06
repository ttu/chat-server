FROM microsoft/dotnet:2.1-sdk AS build

RUN apt-get update && apt-get -y install openssh-server unzip

RUN echo "root:Docker!" | chpasswd

RUN mkdir /var/run/sshd && chmod 0755 /var/run/sshd 
RUN sed -i 's/#PermitRootLogin prohibit-password/PermitRootLogin yes/g' /etc/ssh/sshd_config
RUN sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/g' /etc/ssh/sshd_config
RUN sed -i 's/#StrictModes yes/StrictModes no/g' /etc/ssh/sshd_config

RUN service ssh restart

RUN mkdir /root/.vs-debugger && chmod 0755 /root/.vs-debugger
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v vs2017u1 -l /root/.vs-debugger/

EXPOSE 22 

WORKDIR /app
EXPOSE 5000

COPY . ./

ENTRYPOINT ["dotnet", "watch", "run"]

HEALTHCHECK --interval=1m --timeout=10s --retries=3 CMD curl --silent --fail http://localhost:5000 || exit 1