import {
    LitElement,
    html
    //@ts-ignore
} from "https://unpkg.com/lit-element@2.4.0/lit-element.js?module";

class ProductListItem extends LitElement {
    static get properties() {
        return { product: { type: Object } }
    }

    constructor() {
        super();
        this.product = null
    }

    _handleClick() {
        const evt = new CustomEvent('selected-product', {
            bubbles: true,
            cancelable: true,
            composed: true,
            detail: this.product ? { ...this.product } : null
        });
        //@ts-ignore
        this.dispatchEvent(evt);
    }

    render() {
        return html`
        <section @click="${(e) => this._handleClick()}" style="cursor: pointer">
            <label>Name:</label> <span>${this.product.name}</span> &nbsp;
            <label>Price:</label> <span>${this.product.price}</span>
        </section>
        `
    }
}
//@ts-ignore
customElements.define("spc-product-list-item", ProductListItem);

class ProductItemDetail extends LitElement {
    static get properties() {
        return { product: { type: Object } }
    }

    constructor() {
        super();
        this.product = null
    }

    _handleClick() {
        const evt = new Event('unselect-product', {
            bubbles: true,
            cancelable: true,
            composed: true
        });
        //@ts-ignore
        this.dispatchEvent(evt);
    }


    render() {
        return html`
        <article>
            <h1>${this.product.name}</h1>
            <label>Price: <small>${this.product.price}</small></label>
            <p>
                ${this.product.description || "No description available"}
            </p>
            <button @click="${(e) => this._handleClick()}">Close</button>
        </article>
        `;
    }

}
//@ts-ignore
customElements.define("spc-product-item-detail", ProductItemDetail);


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
        return html`<spc-product-list-item product="${JSON.stringify(product)}"></spc-product-list-item>`;
    }

    detailedTemplate = (product) => {
        return html`<spc-product-item-detail product="${JSON.stringify(product)}"></spc-product-item-detail>`;
    }

    render() {
        return html`
        <article @selected-product="${(e) => (this.detailedProduct = e.detail)}"
            @unselect-product="${(e) => (this.detailedProduct = null)}">
            <slot>
                ${this.paginated.list.length > 0 ? 
                    html`
                    <article>
                        ${this.paginated.list.map(this.getItem)}
                    </article>`
                    : null
                }
            </slot>
            <p>Showing ${this.paginated.list.length} of ${this.paginated.count} products</p>
            ${this.detailedProduct ? this.detailedTemplate(this.detailedProduct) : null}
        </article>
        `;
    }
}

//@ts-ignore
customElements.define("spc-products-list", ProductList);
