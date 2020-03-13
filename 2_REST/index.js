//responses are served over HTTP because SSL
//requests are sent over HTTPS
const http = require('http');
const https = require('https');
const fs = require('fs');

const keys = JSON.parse(fs.readFileSync('keys.json'));
const landing = fs.readFileSync('index.html');


const urls = {
    N2YO: 'https://www.n2yo.com/rest/v1/satellite/positions/',
    ReverseGeocoding: 'https://api.bigdatacloud.net/data/reverse-geocode-client?',
    wikidata: 'https://www.wikidata.org/w/api.php?action=wbgetentities&format=json&props=sitelinks&ids=',
    wikipedia: 'wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro&explaintext&redirects=1&titles='
};

function N2YOUrlBuilder(sat_id) {
    return urls.N2YO + sat_id + '/0/0/0/1/&apiKey=' + keys.N2YO;
}

function SatelliteHTMLBuilder(satData) {
    let name = satData.info.satname;
    let lat = satData.positions[0].satlatitude;
    let lon = satData.positions[0].satlongitude;
    return (`
        <h2>Satellite: ${name}</h2>
            <div>
            Current latitude: ${lat} <br/>
            Current longitude: ${lon} <br/>
        </div>
    `)
}

function RGeocodingUrlBuilder(satData, lang) {
    let lat = satData.positions[0].satlatitude;
    let lon = satData.positions[0].satlongitude;
    return `${urls.ReverseGeocoding}latitude=${lat}&longitude=${lon}&localityLanguage=${lang}`;
}

function InfoElementHTMLBuilder(el) {
    return `
    <li><b>${el.name}</b> - wikidata ID: ${el.wikidataId} </li>
    `;
}

function WikidataUrlBuilder(wikidataID, lang) {
    return `${urls.wikidata}${wikidataID}&sitefilter=${lang}wiki`;
}

function WikipediaUrlBuilder(wikidataData, lang) {
    let Q = Object.values(wikidataData.entities);
    let sitelinks = Q[0].sitelinks;
    let site = Object.values(sitelinks)[0];
    let wikipediaName = site.title;
    let name = encodeURIComponent(wikipediaName.trim())
    return `https://${lang}.${urls.wikipedia}${name}`;
}

function WikipediaHTMLBuilder(wikiEntry) {
    let page = Object.values(wikiEntry.query.pages)[0];
    let paragraph = page.extract;
    return `
    <p>
        ${paragraph}
    </p>
    `
}

const server = http.createServer();

server.on('request', async (req, res) => {
    let q = new URL(req.url, 'http://127.0.0.1/')
    console.log(q.pathname);
    if (q.searchParams.get('sat_id') != null) {
        let satId = q.searchParams.get('sat_id');
        if (satId == '') {
            res.writeHead(400, { 'Content-Type': 'text/html' });
            res.write("<h1>Error 404</h1><br/>Satellite ID not specified!");
            res.end();
        } else {
            let lang = q.searchParams.get('lang');
            lang = lang || 'en';
            console.log(lang);
            res.writeHead(200, { 'Content-Type': 'text/html' });
            //handle satelite data
            res.write('<meta charset="UTF-8">');
            const satData = await getApiData(N2YOUrlBuilder(satId));
            res.write(SatelliteHTMLBuilder(satData));

            //handle reverse geocoding data
            const loc_data = await getApiData(RGeocodingUrlBuilder(satData, lang));
            if (loc_data.localityInfo.administrative != undefined) {
                res.write('<h2>Administrative info:</h2><ul>');
                loc_data.localityInfo.administrative.forEach(el => {
                    res.write(InfoElementHTMLBuilder(el));
                })
                res.write('</ul>');
            }

            if (loc_data.localityInfo.informative != undefined) {
                res.write('<h2>Informative info:</h2><ul>');
                await asyncForEach(loc_data.localityInfo.informative, async el => {
                    res.write(InfoElementHTMLBuilder(el));
                    try{
                        if(el.wikidataId){
                            //handle getting wikipedia name
                            const wikidataData = await getApiData(WikidataUrlBuilder(el.wikidataId, lang));
                            //handle getting wikipedia description
                            const wikipediaParagraph = await getApiData(WikipediaUrlBuilder(wikidataData, lang));
                            res.write(WikipediaHTMLBuilder(wikipediaParagraph));
                        }
                    }catch(error){
                        res.write("Error connecting to Wikipedia. Possible cause: unsupported language");
                    }
                })
                res.write('</ul>');
            }
            res.end();
        }
    } else {
        res.writeHead(200, { 'Content-Type': 'text/html' });
        // res.write("<h1>Landing page</h1><br/>Add form here");
        res.write(landing);
        res.end();
    }
});
server.listen(8080);




function getApiData(apiURL) {
    console.log(apiURL);
    return new Promise((resolve, reject) => {
        https.get(apiURL, (resp) => {
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

//custom (stolen) asynchronous forEach loop
async function asyncForEach(array, callback) {
    for (let index = 0; index < array.length; index++) {
      await callback(array[index], index, array);
    }
  }