import {
  LitElement,
  html,
} from "https://unpkg.com/lit-element@2.4.0/lit-element.js?module";

class SpicySample extends LitElement {
  static get properties() {
    return { name: { type: String } };
  }

  constructor() {
    super();
    this.name = "F#";
  }

  render() {
    return html`<div>Hello ${this.name}!</div>`;
  }
}

customElements.define("spicy-sample", SpicySample);
