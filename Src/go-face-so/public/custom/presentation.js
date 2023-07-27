
// Get the modal
var modal01 = document.getElementById("myModal01");

// Get the image and insert it inside the modal - use its "alt" text as a caption
var img01 = document.getElementById("img-01");
var modalImg01 = document.getElementById("img01");
var captionText01 = document.getElementById("caption01");

img01.onclick = function(){
  modal01.style.display = "block";
  modalImg01.src = this.src;
  captionText01.innerHTML = this.alt;
}

// Get the <span> element that closes the modal
var span01 = document.getElementById("close01");

// When the user clicks on <span> (x), close the modal
span01.onclick = function() { 
  modal01.style.display = "none";
}


// Get the modal
var modal02 = document.getElementById("myModal02");

// Get the image and insert it inside the modal - use its "alt" text as a caption
var img02 = document.getElementById("img-02");
var modalImg02 = document.getElementById("img02");
var captionText02 = document.getElementById("caption02");

img02.onclick = function(){
  modal02.style.display = "block";
  modalImg02.src = this.src;
  captionText02.innerHTML = this.alt;
}

// Get the <span> element that closes the modal
var span02 = document.getElementById("close02");

// When the user clicks on <span> (x), close the modal
span02.onclick = function() { 
  modal02.style.display = "none";
}


// Get the modal
var modal03 = document.getElementById("myModal03");

// Get the image and insert it inside the modal - use its "alt" text as a caption
var img03 = document.getElementById("img-03");
var modalImg03 = document.getElementById("img03");
var captionText03 = document.getElementById("caption03");

img03.onclick = function(){
  modal03.style.display = "block";
  modalImg03.src = this.src;
  captionText03.innerHTML = this.alt;
}

// Get the <span> element that closes the modal
var span03 = document.getElementById("close03");

// When the user clicks on <span> (x), close the modal
span03.onclick = function() { 
  modal03.style.display = "none";
}