window.blPrint = (elementId) => {
    const el = document.getElementById(elementId);
    if (!el) return;

    const printWindow = window.open('', '_blank', 'width=900,height=1000');
    printWindow.document.write(`
        <html>
        <head>
            <title>Bill of Lading</title>
            <style>
                body { font-family: Georgia, serif; padding: 32px; color: #1a1a1a; }
                .mud-divider { border: none; border-top: 1px solid #ddd; margin: 16px 0; }
            </style>
        </head>
        <body>${el.innerHTML}</body>
        </html>
    `);
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 300);
};

window.blDownloadFile = (fileName, contentType, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};