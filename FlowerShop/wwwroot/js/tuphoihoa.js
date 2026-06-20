const dropZone = document.getElementById('drop-zone');
const receiptList = document.getElementById('receipt-list');
const totalPriceEl = document.getElementById('total-price');

const cardMessageInput = document.getElementById('card-message');
const liveCard = document.getElementById('live-card');
const cardTitlePreview = document.getElementById('card-title-preview');
const cardBodyPreview = document.getElementById('card-body-preview');
const cardStyleOptions = document.querySelectorAll('input[name="card-style"]');

let currentBouquet = [];
const FIXED_FEE = 50000;
let activeWrapper = null; // Lưu khung hoa đang được chọn điều khiển

// 1. KÍCH HOẠT SỰ KIỆN KÉO VẬT LIỆU
const materialItems = document.querySelectorAll('.material-item');
materialItems.forEach(item => {
    const imgElement = item.querySelector('img');
    item.addEventListener('dragstart', (e) => {
        e.dataTransfer.setData('id', item.dataset.id);
        e.dataTransfer.setData('name', item.dataset.name);
        e.dataTransfer.setData('price', item.dataset.price);
        e.dataTransfer.setData('img', item.dataset.img);

        const dragIcon = new Image();
        dragIcon.src = item.dataset.img;
        e.dataTransfer.setDragImage(dragIcon, 40, 40);
        if (imgElement) imgElement.style.opacity = '0.4';
    });

    item.addEventListener('dragend', () => {
        if (imgElement) imgElement.style.opacity = '1';
    });
});

dropZone.addEventListener('dragover', (e) => { e.preventDefault(); });

// 2. SỰ KIỆN THẢ HOA VÀO GIẤY GÓI
dropZone.addEventListener('drop', (e) => {
    e.preventDefault();
    const id = e.dataTransfer.getData('id');
    const name = e.dataTransfer.getData('name');
    const rawPrice = e.dataTransfer.getData('price');
    const img = e.dataTransfer.getData('img');

    if (!id || !rawPrice) return;

    const price = parseInt(rawPrice);
    const hint = document.querySelector('.hint-text');
    if (hint) hint.style.display = 'none';

    const rect = dropZone.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    // Gọi hàm tạo khung hoa thông minh
    createInteractiveFlower(id, name, img, x, y);

    // Cập nhật mảng tính tiền
    const itemExist = currentBouquet.find(item => item.id === id);
    if (itemExist) {
        itemExist.quantity += 1;
    } else {
        currentBouquet.push({ id, name, price, quantity: 1 });
    }
    updateReceipt();
});

// 3. HÀM TẠO BÔNG HOA ĐIỀU KHIỂN THÔNG MINH (XOAY - PHÓNG TO - XÓA)
function createInteractiveFlower(id, name, imgSrc, x, y) {
    // Tạo khung bao ngoài wrapper
    const wrapper = document.createElement('div');
    wrapper.classList.add('flower-wrapper');
    wrapper.style.left = `${x - 50}px`; // Căn giữa vị trí chuột
    wrapper.style.top = `${y - 50}px`;
    wrapper.dataset.productId = id; // Lưu ID để phục vụ xóa sau này

    // Các biến lưu trạng thái transform
    let scale = 1;
    let angle = Math.floor(Math.random() * 40) - 20; // Xoay ngẫu nhiên tí cho tự nhiên
    let isDragging = false;
    let startX, startY, startLeft, startTop;

    // Tạo thẻ img chứa bông hoa
    const imgEl = document.createElement('img');
    imgEl.src = imgSrc;
    imgEl.classList.add('placed-flower');
    wrapper.appendChild(imgEl);

    // Tạo nút XÓA (Delete)
    const btnDelete = document.createElement('div');
    btnDelete.className = 'control-handle btn-delete';
    btnDelete.innerHTML = '×';
    btnDelete.title = 'Xóa bông hoa này';
    btnDelete.addEventListener('click', (e) => {
        e.stopPropagation(); // Ngăn sự kiện click lan rộng ra khung wrapper
        wrapper.remove(); // Xóa hoa khỏi màn hình
        removeFlowerFromCart(id); // Trừ tiền trong hóa đơn
    });
    wrapper.appendChild(btnDelete);

    // Tạo nút đa năng: XOAY & PHÓNG TO/THU NHỎ (Transform)
    const btnTransform = document.createElement('div');
    btnTransform.className = 'control-handle btn-transform';
    btnTransform.title = 'Giữ chuột kéo để Xoay / Phóng to thu nhỏ';
    wrapper.appendChild(btnTransform);

    // Hàm cập nhật CSS Transform thời gian thực
    function updateTransform() {
        imgEl.style.transform = `scale(${scale}) rotate(${angle}deg)`;
    }
    updateTransform(); // Chạy khởi tạo góc xoay ban đầu

    // A. LOGIC 1: CLICK CHỌN HOA & KÉO DI CHUYỂN VỊ TRÍ (DRAG MOVE)
    wrapper.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        // Hủy chọn khung cũ, kích hoạt khung hiện tại lên
        if (activeWrapper) activeWrapper.classList.remove('active');
        activeWrapper = wrapper;
        wrapper.classList.add('active');

        isDragging = true;
        startX = e.clientX;
        startY = e.clientY;
        startLeft = parseInt(wrapper.style.left);
        startTop = parseInt(wrapper.style.top);

        document.addEventListener('mousemove', mouseMoveHandler);
        document.addEventListener('mouseup', mouseUpHandler);
    });

    function mouseMoveHandler(e) {
        if (!isDragging) return;
        const dx = e.clientX - startX;
        const dy = e.clientY - startY;
        wrapper.style.left = `${startLeft + dx}px`;
        wrapper.style.top = `${startTop + dy}px`;
    }

    function mouseUpHandler() {
        isDragging = false;
        document.removeEventListener('mousemove', mouseMoveHandler);
        document.removeEventListener('mouseup', mouseUpHandler);
    }

    // B. LOGIC 2: XỬ LÝ XOAY VÀ PHÓNG TO/THU NHỎ KHI KÉO NÚT MÀU XANH
    btnTransform.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        e.preventDefault();

        // Tìm điểm tâm của bông hoa để làm trục xoay
        const rect = wrapper.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;

        // Tính khoảng cách và góc ban đầu từ tâm hoa đến vị trí chuột click
        const startRadius = Math.hypot(e.clientX - centerX, e.clientY - centerY);
        const startAngle = Math.atan2(e.clientY - centerY, e.clientX - centerX) * (180 / Math.PI);
        
        const baseScale = scale;
        const baseAngle = angle;

        function transformMoveHandler(moveEvent) {
            // Khoảng cách và góc hiện tại của con chuột khi di chuyển
            const currentRadius = Math.hypot(moveEvent.clientX - centerX, moveEvent.clientY - centerY);
            const currentAngle = Math.atan2(moveEvent.clientY - centerY, moveEvent.clientX - centerX) * (180 / Math.PI);

            // 1. Tính toán Tỉ lệ phóng to thu nhỏ (Scale)
            scale = baseScale * (currentRadius / startRadius);
            // Giới hạn không cho hoa nhỏ quá hoặc to quá che hết màn hình
            if (scale < 0.4) scale = 0.4;
            if (scale > 3.0) scale = 3.0;

            // 2. Tính toán Góc xoay (Rotate)
            angle = baseAngle + (currentAngle - startAngle);

            // Áp dụng trực tiếp lên hình ảnh hoa
            updateTransform();
        }

        function transformUpHandler() {
            document.removeEventListener('mousemove', transformMoveHandler);
            document.removeEventListener('mouseup', transformUpHandler);
        }

        document.addEventListener('mousemove', transformMoveHandler);
        document.addEventListener('mouseup', transformUpHandler);
    });

    dropZone.appendChild(wrapper);
}

// Click ra ngoài khoảng trống của giấy gói thì ẩn các nút điều khiển đi cho đẹp
dropZone.addEventListener('mousedown', () => {
    if (activeWrapper) {
        activeWrapper.classList.remove('active');
        activeWrapper = null;
    }
});

// 4. HÀM XỬ LÝ TRỪ TIỀN KHI ADMIN/KHÁCH XÓA BỚT HOA TRÊN GIẤY GÓI
function removeFlowerFromCart(id) {
    const itemExist = currentBouquet.find(item => item.id === id);
    if (itemExist) {
        if (itemExist.quantity > 1) {
            itemExist.quantity -= 1;
        } else {
            // Nếu chỉ còn 1 bông thì xóa hẳn sản phẩm đó khỏi mảng hóa đơn
            currentBouquet = currentBouquet.filter(item => item.id !== id);
        }
    }
    updateReceipt(); // Cập nhật lại hóa đơn tính tiền bên phải
}

function changeWrap(wrapClass) { dropZone.className = `bouquet-zone ${wrapClass}`; }

function updateReceipt() {
    receiptList.innerHTML = '';
    let subTotal = 0;
    currentBouquet.forEach(item => {
        const li = document.createElement('li');
        li.innerHTML = `<div><b>${item.name}</b></div><div>x${item.quantity} - ${(item.price * item.quantity).toLocaleString()}đ</div>`;
        receiptList.appendChild(li);
        subTotal += item.price * item.quantity;
    });
    totalPriceEl.innerText = (subTotal + FIXED_FEE).toLocaleString();
}

// GỬI DỮ LIỆU ĐẾN BACKEND
// GỬI DỮ LIỆU ĐẾN BACKEND (ĐÃ VÁ LỖI MAPPING)
function sendCartToBackend() {
    const dropZone = document.getElementById("drop-zone");

    // 1. Khử rác giao diện: Ẩn toàn bộ viền active, nút xóa x, nút xoay xanh trước khi bấm máy chụp
    const allWrappers = document.querySelectorAll('.flower-wrapper');
    allWrappers.forEach(el => el.classList.remove('active'));
    if (activeWrapper) activeWrapper.classList.remove('active');

    // Chờ 50ms rất ngắn để trình duyệt kịp xóa các class active rồi mới bắt đầu chụp
    setTimeout(() => {
        html2canvas(dropZone, {
            useCORS: true,
            scale: 2, // Tăng gấp đôi độ phân giải giúp ảnh mẫu ngoài giỏ hàng sắc nét, không vỡ hạt
            backgroundColor: "#fffafb" // Ép nền hồng nhạt đồng bộ cho bó hoa thiết kế
        }).then(canvas => {
            // Chuyển toàn bộ bức ảnh phối trơn sạch sẽ thành chuỗi Base64 thần thánh
            const base64Image = canvas.toDataURL("image/png");

            // 2. Thu thập dữ liệu hoa lẻ thực tế dựa trên các bông hoa đang cắm trên giấy gói
            let payloadItems = [];
            const placedFlowers = document.querySelectorAll('.flower-wrapper');

            if (placedFlowers.length === 0) {
                alert("Bó hoa tự phối của má chưa có nguyên liệu nào, kéo thả vài bông vào đã nhé!");
                return;
            }

            // Đếm số lượng thực tế của từng ProductId trên giao diện Studio Canvas
            let counts = {};
            placedFlowers.forEach(el => {
                let pid = parseInt(el.dataset.productId);
                if (pid) { counts[pid] = (counts[pid] || 0) + 1; }
            });

            // Đẩy vào mảng với thuộc tính viết thường khớp chính xác với DTO nhận ở Backend
            for (let pid in counts) {
                payloadItems.push({
                    productId: parseInt(pid),
                    quantity: counts[pid]
                });
            }

            // 3. Nhận diện loại giấy gói hiện tại của khách
            let currentWrapText = "Giấy Hồng";
            if (dropZone.classList.contains('wrap-vintage')) currentWrapText = "Kraft Cổ Điển";
            if (dropZone.classList.contains('wrap-black')) currentWrapText = "Đen Huyền Bí";

            // Bọc luôn nội dung chữ viết thiệp kỷ niệm/sinh nhật (nếu có) đi kèm vào ghi chú
            const cardMessage = cardMessageInput.value.trim();
            let finalWrapText = currentWrapText;
            if (cardMessage.length > 0) {
                finalWrapText += ` (Kèm thiệp: ${cardTitlePreview.innerText} - ${cardMessage})`;
            }

            // 4. Đóng gói Payload gửi đi
            const payload = {
                wrapName: finalWrapText,
                items: payloadItems,
                customImage: base64Image // Đính kèm chuỗi Base64 ảnh trọn bộ bó hoa vừa chụp
            };

            // 5. Bắn API sang CartController
            fetch('/Cart/AddCustomBouquet', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert("Đã lưu tác phẩm thiết kế độc bản kèm ảnh chụp vào giỏ hàng thành công! 📸💐");
                        window.location.href = "/ShoppingCart/Index"; // Chuyển hướng về trang giỏ hàng
                    } else {
                        alert("Có lỗi xảy ra: " + data.message);
                    }
                })
                .catch(err => {
                    alert("Lỗi kết nối hệ thống Backend rồi má ơi!");
                    console.error(err);
                });
        });
    }, 50);
}

// Xử lý viết thiệp thời gian thực (Real-time)
cardMessageInput.addEventListener('input', function () {
    const textValue = cardMessageInput.value.trim();
    if (textValue.length > 0) {
        liveCard.classList.add('active');
        cardBodyPreview.innerText = cardMessageInput.value;
    } else {
        liveCard.classList.remove('active');
        cardBodyPreview.innerText = "Lời chúc của bạn sẽ hiện ở đây...";
    }
});

cardStyleOptions.forEach(radio => {
    radio.addEventListener('change', function () {
        cardTitlePreview.innerText = this.value;
        if (liveCard.classList.contains('active')) {
            liveCard.style.transform = 'translateY(0) scale(1.05)';
            setTimeout(() => {
                liveCard.style.transform = 'translateY(0) scale(1)';
            }, 150);
        }
    });
});