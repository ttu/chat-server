# The Node version that we'll be running for our version of React.
# You may have to search the Node directory for a version that fits
# the version of React you're using.
FROM node:latest

# Create a work directory and copy over our dependency manifest files.
RUN mkdir /app
WORKDIR /app

COPY /src /app/src
COPY /public /app/public
COPY ["package.json", "package-lock.json*", "./"]

# If you're using yarn:
#  yarn build
# RUN npm install --production --silent && mv node_modules ../
RUN npm install

# ADD . /usr/src/app

# Expose PORT 3000 on our virtual machine so we can run our server
EXPOSE 3000

CMD ["npm", "start"]