(() => {

  async function updateCharacters() {
    let characters = await (await fetch('/characters')).json();
    let trs = characters.map((c) => {
      let tr = document.createElement('tr');
      let tdA = document.createElement('td');
      let imgA = document.createElement('img');
      imgA.src = `/assets/avatars/${c.Avatar}.png`;
      imgA.height = 24;
      imgA.width = 24;
      tdA.appendChild(imgA);
      tr.appendChild(tdA);
      let tdN = document.createElement('td');
      tdN.appendChild(document.createTextNode(c.Name));
      tr.appendChild(tdN);
      let tdW = document.createElement('td');
      tdW.appendChild(document.createTextNode(c.Wellbeing));
      tdW.classList.add("wellbeing");
      tdW.classList.add(c.Wellbeing < 0 ? "wellbeing--negative" : "wellbeing--positive");
      tr.appendChild(tdW);
      return tr;
    });
    document.getElementById('characters').replaceChildren(...trs);

    window.setTimeout(updateCharacters, 10000);
  }

  updateCharacters();
})();
