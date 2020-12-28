
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
/**
 * @this {HTMLDivElement}
 * @param {MouseEvent} event 
 */
function removeElement(event) {
  const target = event.currentTarget;
  /**
   * @type {HTMLElement}
   */
  const parent = target.parentElement;
  target.removeEventListener("click", removeElement);
  parent.remove();
}


document.addEventListener("DOMContentLoaded", function (event) {
  var items = Array.from(document.querySelectorAll(".navbar-burger"));
  items.forEach(function (navbar) {
    navbar.addEventListener("click", navbarClick);
  });

  var deletes = Array.from(document.querySelectorAll(".delete"))
  deletes.forEach(function(del) {
    del.addEventListener("click", removeElement);
  });
});

document.body.addEventListener("htmx:configRequest", function (evt) {
  var form = document.querySelector('input[name="__RequestVerificationToken"]');
  if (
    form &&
    ["post", "patch", "put", "delete"].includes(evt.detail.verb.toLowerCase())
  ) {
    evt.detail.headers["XSRF-TOKEN"] = form.value;
  }
});

document.body.addEventListener("htmx:responseError", function (evt) {
  /**
   * @type {HTMLElement}
   */
  const target = evt.detail.target;
  if(target === document.body) {
    const main = target.querySelector('.app-main');
    main.innerHTML = evt.detail.xhr.response;
  } else {
    target.innerHTML = evt.detail.xhr.response;
  }

  if (evt.detail.elt === document.body) {
    var items = Array.from(document.querySelectorAll(".navbar-burger"));
    items.forEach(function (navbar) {
      navbar.addEventListener("click", navbarClick);
    });
  }

  var deletes = Array.from(document.querySelectorAll("button.delete"))
  deletes.forEach(function(del) {
    del.addEventListener("click", removeElement);
  });
});

document.body.addEventListener("htmx:load", function (evt) {
  if (evt.detail.elt === document.body) {
    var items = Array.from(document.querySelectorAll(".navbar-burger"));
    items.forEach(function (navbar) {
      navbar.addEventListener("click", navbarClick);
    });
  }

  var deletes = Array.from(document.querySelectorAll("button.delete"))
  deletes.forEach(function(del) {
    del.addEventListener("click", removeElement);
  });
});
