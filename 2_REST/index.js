//websites are served over HTTP because SSL
var http = require('http');
var https = require('https');

const url = 'https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY';

const server = http.createServer();

server.on('request', async (req, res) => {
    res.writeHead(200, {'Content-Type': 'text/html'});
    const x = await getApiData(url);
    res.write(JSON.stringify(x));
    res.end();
  });
server.listen(8080);

function getApiData(url){
    return new Promise((resolve, reject) => {
        https.get(url, (resp) => {
            let data = '';
            // A chunk of data has been recieved.
            resp.on('data', (chunk) => {
            data += chunk;
            });
        
            // The whole response has been received. Print out the result.
            resp.on('end', () => {
            // console.log(JSON.parse(data).explanation);
            payload = JSON.parse(data);
            resolve(payload);
            });
        
        }).on("error", (err) => {
            reject(err);
            console.log("Error: " + err.message);
        });
    })

}

async function waitForGet(url, res){
    const x = await getApiData(url);
    res.write(x);
    res.end();
}

// waitForGet('https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY');

