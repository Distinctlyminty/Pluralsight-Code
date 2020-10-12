
var cartId = "shoppingCart";

  var db = new PouchDB('ProductsDatabase');

var localAdapter = {

    saveCart: function (object) {

        var stringified = JSON.stringify(object);
        localStorage.setItem(cartId, stringified);
        return true;

    },
    getCart: function () {

        return JSON.parse(localStorage.getItem(cartId));

    },
    clearCart: function () {

        localStorage.removeItem(cartId);

    },
    saveProductData: function(object){
        // add the _id property to our json
        object._id = "productData";

        // does the document already exist in the database?
        db.get('productData').catch(function (error) {
            if (error.name === 'not_found') {
              db.put(object);
            } else { 
              throw error;
            }
          }).then(function (doc) {
              object._rev = doc._rev;
              db.put(object);
          }).catch(function (error) {
            console.log(error)
          });
        return true; 
    },
    
     getProducts:  function(){
            return db.get('productData')
            .catch(function (error) {
                if (error.name === 'not_found') {
                  console.log('No products found!');
                } else{
                    console.log(error);
                }
        });
         
         },
    saveLastFetched: function(date){
        localStorage.setItem("lastFetched",date);
        return true;
    },
    getLastFetched: function(){
        return localStorage.getItem("lastFetched");
    },
    saveNextUpdate: function(date){
        localStorage.setItem("nextUpdate",date);
        return true;
    },
    getNextUpdate: function(){
        return localStorage.getItem("nextUpdate");
    }

};

var storage = localAdapter;

const getProducts = async () => {
    const url = 'http://127.0.0.1:5500/ProductService.json'
    await fetch(url)
    .then((resp) => resp.json()) // Transform the data into json
    .then(function(data) {
      localAdapter.saveProductData(data);
      localAdapter.saveNextUpdate(addOneHour(data.LastUpdatedDate));
      
      })
}

function addOneHour(date){
    var nextUpdate = new Date(date);
    nextUpdate.setHours(nextUpdate.getHours() + 1);
     return nextUpdate.toString();
}

const pollProductsAPI = async ({ interval }) => {
    console.log('Start polling');

    const executePoll = async () => {
      console.log('- polling products');
    
      const nextUpdate = storage.getNextUpdate();
      var nextUpdateDate = new Date(nextUpdate);
     
if(!nextUpdate || nextUpdateDate < new Date()){
    console.log('Data has expired - calling the API!');
const result = await getProducts();

}
   setTimeout(executePoll, interval);
      
};

    return new Promise(executePoll);
  };

  function isDateBeforeToday(date) {
    var fetchedDateString = date.toDateString();
    var todayString = new Date(new Date().toDateString());
  return new Date(date.toDateString()) < new Date(new Date().toDateString());
}

var helpers = {

    getHtml: function (id) {

        return document.getElementById(id).innerHTML;

    },
    setHtml: function (id, html) {

        document.getElementById(id).innerHTML = html;
        return true;

    },
    itemData: function (object) {

        var count = object.querySelector(".count"),
            patt = new RegExp("^[1-9]([0-9]+)?$");
        count.value = (patt.test(count.value) === true) ? parseInt(count.value) : 1;

        var item = {

            name: object.getAttribute('data-name'),
            price: object.getAttribute('data-price'),
            id: object.getAttribute('data-id'),
            count: count.value,
            total: parseInt(object.getAttribute('data-price')) * parseInt(count.value)

        };
        return item;

    },
    updateView: function () {

        var items = cart.getItems(),
            template = this.getHtml('cartTemplate'),
            compiled = _.template(template, {
                items: items
            });
        this.setHtml('cartItems', compiled);
        this.updateTotal();

    },
    emptyView: function () {

        this.setHtml('cartItems', '<p>Add some items to see</p>');
        this.updateTotal();

    },
    updateTotal: function () {

        this.setHtml('totalPrice',  '£' +cart.total.toFixed(2));

    },
     updateProducts:  async function (){
         
    var results = await localAdapter.getProducts();

                        var products =   results.ProductsList,
                        template = helpers.getHtml('productTemplate'),
                        compiled = _.template(template, {
                            items: products
                        });
                        helpers.setHtml('main', compiled)

                        var products = document.querySelectorAll('.product button');
[].forEach.call(products, function (product) {

    product.addEventListener('click', function (e) {

        var item = helpers.itemData(this.parentNode);
        cart.addItem(item);

    })
});
      }

};

var cart = {

    count: 0,
    total: 0,
    items: [],
    getItems: function () {

        return this.items;

    },
    setItems: function (items) {

        this.items = items;
        for (var i = 0; i < this.items.length; i++) {
            var _item = this.items[i];
            this.total += _item.total;
        }

    },
    clearItems: function () {

        this.items = [];
        this.total = 0;
        storage.clearCart();
        helpers.emptyView();

    },
    addItem: function (item) {

        if (this.containsItem(item.id) === false) {

            this.items.push({
                id: item.id,
                name: item.name,
                price: item.price,
                count: item.count,
                total: item.price * item.count
            });

            storage.saveCart(this.items);


        } else {

            this.updateItem(item);

        }
        this.total += item.price * item.count;
        this.count += item.count;
        helpers.updateView();

    },
    containsItem: function (id) {

        if (this.items === undefined) {
            return false;
        }

        for (var i = 0; i < this.items.length; i++) {

            var _item = this.items[i];

            if (id == _item.id) {
                return true;
            }

        }
        return false;

    },
    updateItem: function (object) {

        for (var i = 0; i < this.items.length; i++) {

            var _item = this.items[i];

            if (object.id === _item.id) {

                _item.count = parseInt(object.count) + parseInt(_item.count);
                _item.total = parseInt(object.total) + parseInt(_item.total);
                this.items[i] = _item;
                storage.saveCart(this.items);

            }

        }

    }

};

document.addEventListener('DOMContentLoaded', function () {

pollProductsAPI({interval:10000}).then(helpers.updateProducts())
   
    if (storage.getCart()) {

        cart.setItems(storage.getCart());
        helpers.updateView();

    } else {

        helpers.emptyView();

    }
    

    document.querySelector('#clear').addEventListener('click', function (e) {

        cart.clearItems();

    });

}); 