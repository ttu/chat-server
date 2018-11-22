import React, { Component } from "react";
import "./App.css";

import client from "./client.js";

const CHAT_URL = process.env.REACT_APP_CHAT_URL || 'http://localhost:5000/api';
const CHAT_WS = process.env.REACT_APP_CHAT_WS || 'ws://localhost:5000/ws';

class App extends Component {
  constructor(props) {
    super(props);
    this.state = {
      name: "",
      temp: "Calvin",
      message: "",
      receiver: "B",
      received: []
    };
  }

  componentDidMount() {
    this.connection = new WebSocket(CHAT_WS);

    this.connection.onmessage = evt => {
      this.setState(state => ({ received: [...state.received, evt.data] }));
      console.log(JSON.stringify(evt.data));
    };
  }

  handleChange = (event, propName) => {
    this.setState({
      [propName]: event.target.value
    });
    event.preventDefault();
  };

  setName = () => {
    this.setState(state => ({
      name: state.temp
    }));
    this.connection.send("username:" + this.state.temp);
  };

  sendMessage = () => {
    client.postData(`${CHAT_URL}/send`, this.state.name, {
      receiver: this.state.receiver,
      payload: this.state.message
    });
  };

  render() {
    return (
      <div className="App">
        <header className="App-header">

          <p>Hello: {this.state.name}</p>

          <input
            type="text"
            value={this.state.temp}
            onChange={e => this.handleChange(e, 'temp')}
          />
          <input type="submit" value="Set name" onClick={this.setName} />

          <p>Receiver: {this.state.receiver}</p>

          <input
            type="text"
            value={this.state.receiver}
            onChange={e => this.handleChange(e, 'receiver')}
          />

          <p>Message:</p>

          <input
            type="text"
            value={this.state.message}
            onChange={e => this.handleChange(e, 'message')}
          />
          <input type="submit" value="Send" onClick={this.sendMessage} />

          <p>Received:</p>
          <div>
            {this.state.received.map(line => (
              <div>{JSON.stringify(line)}</div>
            ))}
          </div>

        </header>
      </div>
    );
  }
}

export default App;
