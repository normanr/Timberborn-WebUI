document.addEventListener("DOMContentLoaded", () => {
  let viewModel = {
    characters: ko.observableArray(),
  }
  ko.applyBindings(viewModel);

  async function updateCharacters() {
    let characters = await (await fetch('/characters')).json();
    viewModel.characters.splice(0, Infinity, ...characters);
    window.setTimeout(updateCharacters, 10000);
  }

  updateCharacters();
});
