(function () {
    let connection;
    this.connection = connection;

    connection = new signalR.HubConnectionBuilder().withUrl("/weatherHub").build();

    connection.on('ReceiveUpdate', (cityId, temperature) => {
        document.querySelector(`[data-temperature-for='${cityId}']`).innerHTML = '' + temperature;
    });

    connection.start().then(() => {
        let subs = [...document.querySelectorAll('[data-temperature-for]')].map(el => (
            connection.invoke('SubscribeCity', parseInt(el.dataset.temperatureFor))
        ))
        return Promise.all(subs);
    }).then(
        s => console.log(s)
    ).catch(function (err) {
        return console.error(err.toString());
    });
})()