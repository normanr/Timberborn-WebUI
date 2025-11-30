document.addEventListener("DOMContentLoaded", () => {
  let viewModel = {
    characters: ko.observableArray(),
    goods: ko.observableArray(),
  }
  ko.applyBindings(viewModel);

  async function update() {
    await Promise.all([
      (async () => {
        let characters = await (await fetch('/characters')).json();
        viewModel.characters.splice(0, Infinity, ...characters);
      })(),
      (async () => {
        let goods = await (await fetch('/goods')).json();
        viewModel.goods.splice(0, Infinity, ...goods);
      })(),
    ])
    window.setTimeout(update, 10000);
  }

  update();
});
