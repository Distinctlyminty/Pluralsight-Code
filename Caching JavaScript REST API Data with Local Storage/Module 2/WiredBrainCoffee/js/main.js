const input = document.querySelector('#name');
const form = document.querySelector('form');
const submit = document.querySelector('#submitName');
const remove = document.querySelector('#clear');

var storage = sessionStorage;

form.addEventListener('submit', (e) => {
    e.preventDefault();
});

submit.addEventListener('click', () => {
    storage.setItem('name', input.value);
    updateNameTitle();
});

remove.addEventListener('click', () => {
    storage.removeItem('name');
    updateNameTitle();
});


function updateNameTitle(){
    const name = storage.getItem('name');

const h1 = document.querySelector('#title');

name ? h1.textContent = `Welcome ${name}` : h1.textContent = '';
}
