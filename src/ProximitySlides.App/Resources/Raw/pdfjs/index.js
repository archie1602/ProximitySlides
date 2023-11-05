const queryParams = new Proxy(new URLSearchParams(window.location.search), {
	get: (searchParams, prop) => searchParams.get(prop)
});

const pathToPdf = queryParams.file;
const pdfPage = queryParams.page == null ? 1 : parseInt(queryParams.page);

// Get the PDF file URL.
// const pdfUrl = './assets/Introduction_to_Hadoop_slides.pdf';

pdfjsLib.GlobalWorkerOptions.workerSrc = './build/pdf.worker.js';

// Load the PDF file using PDF.js.
pdfjsLib.getDocument(pathToPdf).promise.then(function (pdfDoc) {
	// Get the first page of the PDF file.
	pdfDoc
		.getPage(pdfPage)
		.then(function (page) {
			const viewer = document.getElementById('pdf-viewer');
			const canvas = document.createElement("canvas");

			canvas.className = 'pdf-page-canvas';

			viewer.appendChild(canvas);

			// const viewport = page.getViewport({ scale: 1 });

			// canvas.width = window.screen.width;
			// console.log(window.screen.width);
			
			const scale = 1.0;
			const viewport = page.getViewport({ scale: window.screen.width / page.getViewport({ scale: scale }).width});

			canvas.height = viewport.height;
			canvas.width = viewport.width;
			canvas.style.height = viewport.height;
			canvas.style.width = viewport.width;

			// canvas.height = viewport.height;
			// canvas.width = viewport.width;

			const ctx = canvas.getContext('2d');

			const renderContext = {
				canvasContext: ctx,
				viewport: viewport
			};

			page.render(renderContext);
		})
		.then(function () {
			console.log('Rendering complete');
		})
		.catch(function (error) {
			console.log('Error loading PDF file:', error);
		});
});

// // Load the PDF file using PDF.js.
// pdfjsLib.getDocument(pdfUrl).promise.then(function (pdfDoc) {
// 	// Get the first page of the PDF file.
// 	pdfDoc
// 		.getPage(2)
// 		.then(function (page) {
// 			const viewport = page.getViewport({ scale: 1 });
//
// 			// Support HiDPI-screens.
// 			const outputScale = window.devicePixelRatio || 1;
//
// 			// Set the canvas dimensions to match the PDF page size.
// 			// canvas.width = viewport.width;
// 			// canvas.height = viewport.height;
//
// 			// Set the canvas rendering context.
// 			const ctx = canvas.getContext('2d');
//
// 			canvas.width = Math.floor(viewport.width * outputScale);
// 			canvas.height = Math.floor(viewport.height * outputScale);
// 			canvas.style.width = Math.floor(viewport.width) + "px";
// 			canvas.style.height = Math.floor(viewport.height) + "px";
//
// 			const transform = outputScale !== 1
// 			? [outputScale, 0, 0, outputScale, 0, 0]
// 			: null;
//
// 			const renderContext = {
// 				canvasContext: ctx,
// 				transform: transform,
// 				viewport: viewport
// 			};
//
// 			// Render the PDF page to the canvas.
// 			page.render(renderContext);
// 		})
// 		.then(function () {
// 			console.log('Rendering complete');
// 		})
// 		.catch(function (error) {
// 			console.log('Error loading PDF file:', error);
// 		});
// });