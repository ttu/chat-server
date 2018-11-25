FROM node:11.1.0

RUN mkdir /server
WORKDIR /server

COPY ["package.json", "package-lock.json*", "./"]
RUN npm install

COPY . /server

EXPOSE 5590
CMD [ "npm", "run", "watch" ]