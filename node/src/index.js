const { ErrorMonitoringClient } = require('./lib/client');

let singleton = null;

function init(options = {}) {
  singleton = new ErrorMonitoringClient(options);
  singleton.start();
  return singleton;
}

function getClient() {
  return singleton;
}

module.exports = {
  init,
  getClient,
  ErrorMonitoringClient,
  createClient: (options = {}) => new ErrorMonitoringClient(options)
};
