# The Node version that we'll be running for our version of React.
# You may have to search the Node directory for a version that fits
# the version of React you're using.
FROM node:11.1.0-alpine

# Create a work directory and copy over our dependency manifest files.
RUN mkdir /app
WORKDIR /app

COPY ["package.json", "package-lock.json*", "./"]
RUN npm install

COPY /src /app/src
COPY /public /app/public

# Expose PORT 3000 on our virtual machine so we can run our server
EXPOSE 3000
EXPOSE 35729

CMD ["npm", "start"]