document.body.addEventListener("htmx:configRequest", function (evt) {
  var form = document.querySelector('input[name="__RequestVerificationToken"]');
  if (
    form &&
    ["post", "patch", "put", "delete"].includes(evt.detail.verb.toLowerCase())
  ) {
    evt.detail.headers["XSRF-TOKEN"] = form.value;
  }
});

/**
 * @this {HTMLAnchorElement}
 * @param {*} event
 */
function navbarClick(event) {
  this.classList.toggle("is-active");
  if (this.parentElement && this.parentElement.parentElement) {
    var menu = this.parentElement.parentElement.querySelector(
      ".navbar-menu"
    ) || { classList: {} };
    menu.classList.toggle("is-active");
  }
}

document.addEventListener("DOMContentLoaded", function (event) {
  var items = Array.from(document.querySelectorAll(".navbar-burger"));
  items.forEach(function (navbar) {
    navbar.addEventListener("click", navbarClick);
  });
});

document.body.addEventListener("htmx:load", function (evt) {
  if (evt.detail.elt === document.body) {
    var items = Array.from(document.querySelectorAll(".navbar-burger"));
    items.forEach(function (navbar) {
      navbar.addEventListener("click", navbarClick);
    });
  }
});
