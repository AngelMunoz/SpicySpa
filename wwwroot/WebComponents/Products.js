import {
    LitElement,
    html
    //@ts-ignore
} from "https://unpkg.com/lit-element@2.4.0/lit-element.js?module";


class ProductList extends LitElement {

    static get properties() {
        return { paginated: { type: Object }, detailedProduct: { type: Object, attribute: false } };
    }

    constructor() {
        super();
        /**
         * @type {PaginatedResult<Product>}
         */
        this.paginated = { list: [], count: 0 };
        /**
         * @type {Product | null}
         */
        this.detailedProduct = null;
    }

    getItem = (product) => {
        return html`
        <li @click="${(e) => (this.detailedProduct = product)}" style="cursor: pointer">
            <label>Name:</label> <span>${product.name}</span> &nbsp;
            <label>Price:</label> <span>${product.price}</span>
        </li>
        `;
    }

    detailedTemplate = (detailed) => {
        return html`
        <h1>${detailed.name}</h1>
        <label>Price: <small>${detailed.price}</small></label>
        <p>
            ${detailed.description || "No description available"}
        </p>
        <button @click="${(e) => (this.detailedProduct = null)}">Close</button>
        `;
    }

    render() {
        return html`
        <ul>
            ${this.paginated.list.map(this.getItem)}
        </ul>
        <p>Showing ${this.paginated.list.length} of ${this.paginated.count} products</p>
        ${this.detailedProduct ? this.detailedTemplate(this.detailedProduct) : null}
        `;
    }
}

//@ts-ignore
customElements.define("spc-products-list", ProductList);
