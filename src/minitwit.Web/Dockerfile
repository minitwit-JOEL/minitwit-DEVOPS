#Use Node.js image
FROM node:18-alpine

#Set working directory
WORKDIR /app

#Copy package.json and install dependencies
COPY package.json package-lock.json ./
RUN npm install

#Copy the rest of the project files
COPY . .

#Build the application
RUN npm run build

#Expose port for the frontend
EXPOSE 3000

#Start the Remix app
CMD ["npm", "run", "start"]