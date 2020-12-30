import {
    LitElement,
    html,
    css
    //@ts-ignore
} from "https://unpkg.com/lit-element@2.4.0/lit-element.js?module";
import { get } from "../utils.js";

class ProductListItem extends LitElement {
    static get properties() {
        return { product: { type: Object } }
    }

    constructor() {
        super();
        this.product = null
    }

    
    render() {
        return html`
        <section @click="${(e) => this._handleClick()}" style="cursor: pointer">
            <label>Name:</label> <span>${this.product.name}</span> &nbsp;
            <label>Price:</label> <span>${this.product.price}</span>
        </section>
        `
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
}
//@ts-ignore
customElements.define("spc-product-list-item", ProductListItem);

class ProductItemDetail extends LitElement {
    static get styles() {
        return css`
        b {
            font-size: 2rem;
        }
        `;
    }

    static get properties() {
        return { product: { type: Object } }
    }

    constructor() {
        super();
        this.product = null
    }

    render() {
        return html`
        <article>
            <b>${this.product.name}</b> <br>
            <label>Price: <small>${this.product.price}</small></label>
            <p>
                ${this.product.description || "No description available"}
            </p>
            <button @click="${(e) => this._handleClick()}">Close</button>
        </article>
        `;
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
}
//@ts-ignore
customElements.define("spc-product-item-detail", ProductItemDetail);

class ProductList extends LitElement {

    static get styles() {
        return css`
        :host {
            margin: 1em;
        }
        article {
            display: flex;
            flex-direction: row;
            padding: 1em;
        }
        .pagination {
            padding: 0 1em;
        }
        .product-list {
            display: flex;
            flex-direction: column;
            justify-content: flex-start;
            margin-right: 0.5em;
            min-height: 200px;
        }
        .product-detail {
            display: flex;
            flex-direction: column;
            justify-content: center;
            margin-left: 0.5em;
        }
        `;
    }

    static get properties() {
        return { 
            paginated: { type: Object },
            page: {type: Number, reflect: true },
            limit: {type: Number, reflect: true },
            hasSsrContent: {type: Boolean, reflect: true, attribute: 'has-ssr-content'},
            detailedProduct: { type: Object, attribute: false },
            hasRequested: {type: Number, attribute: false }

        };
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
        this.page = 1;
        this.limit = 5
        this.hasRequested = false;
        this.hasSsrContent = false;
    }

    get defaultSlot() {
        //@ts-ignore
        return this.shadowRoot.querySelector('slot');
    }

    get isPrevDisabled() {
        return !(this.page > 1)
    }

    get isNextDisabled() {
        return !((this.page * this.limit) < this.paginated.count)
    }

    get showing() {
        return this.paginated.list.length ||
            //@ts-ignore
            this.children.length;
    }


    getItem(product) {
        return html`<spc-product-list-item product="${JSON.stringify(product)}"></spc-product-list-item>`;
    }

    detailedTemplate(product) {
        return html`
        <section class="product-detail">
            <spc-product-item-detail product="${JSON.stringify(product)}"></spc-product-item-detail>
        </section>
        `;
    }

    async _previous(e) {

        try {
            const result = await get(`/api/products?page=${(this.page || 2) - 1}&limit=${this.limit || 5}`)
            // we just requested json data
            // remove any server side rendered stuff to show the new data right away
            if (this.hasSsrContent && this.defaultSlot) {
                this.defaultSlot.remove();
                this.hasSsrContent = false;
            }
            this.paginated = { ...result };
            this.page -= 1;
        } catch (error) {
            console.warn({ error });
        }
    }

    async _next(e) {

        try {
            const result = await get(`/api/products?page=${(this.page || 1) + 1}&limit=${this.limit || 5}`)
            // we just requested json data
            // remove any server side rendered stuff to show the new data right away
            if (this.hasSsrContent && this.defaultSlot) {
                this.defaultSlot.remove();
                this.hasSsrContent = false;
            }
            this.paginated = { ...result };
            this.page += 1;
        } catch (error) {
            console.warn({ error });
        }
    }
    render() {
        return html`
        <article
            @selected-product="${(e) => (this.detailedProduct = e.detail)}"
            @unselect-product="${(e) => (this.detailedProduct = null)}">
            <section class="product-list">
            ${this.paginated.list.length > 0 ? 
                html`
                    ${this.paginated.list.map(this.getItem)}`
                    : null
                }
            <slot></slot>
            </section>
            ${this.detailedProduct ? this.detailedTemplate(this.detailedProduct) : null}
        </article>
        <aside class="pagination">
            <p>Showing ${this.showing} of ${this.paginated.count} products</p>
            <button .disabled="${this.isPrevDisabled}" @click="${this._previous}">Prev</button>
            <button .disabled="${this.isNextDisabled}" @click="${this._next}">Next</button>
        </aside>
        `;
    }

}

//@ts-ignore
customElements.define("spc-products-list", ProductList);
