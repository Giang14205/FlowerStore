const chatWindow = document.getElementById('chatWindow');
const chatBody = document.getElementById('chatBody');
const userInput = document.getElementById('userInput');
let isFirstOpen = true;

function toggleChat() {
    chatWindow.classList.toggle('active');
    if (isFirstOpen && chatWindow.classList.contains('active')) {
        // Delay 1 chút cho tự nhiên
        setTimeout(() => {
            addBotMessage("Hế lô! 🎋 <b>PanPan</b> đang nghỉ tay gặm tre thì thấy bạn ghé chơi! 🐼✨<br><br>Bạn đang tìm <b>bó hoa xinh tặng mẹ nhân ngày đặc biệt, hay hoa xinh để tặng bạn gái phải không?</b> 🌸 Kể PanPan nghe đi! 💕");
        }, 500);
        isFirstOpen = false;
    }
}

function handleEnter(e) {
    if (e.key === 'Enter') sendMessage();
}

function sendQuickReply(text) {
    userInput.value = text;
    sendMessage();
}

async function sendMessage() {
    const text = userInput.value.trim();
    if (!text) return;

    // 1. Hiển thị tin nhắn của người dùng
    addUserMessage(text);
    userInput.value = '';

    // 2. Hiển thị hiệu ứng "PanPan đang gõ..." (Loading)
    const loadingId = addBotLoading();

    try {
        // 3. Gọi API lên Server (C# Controller)
        const response = await fetch('/Chatbot/GetResponse', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ message: text })
        });

        // Xóa hiệu ứng loading
        removeMessage(loadingId);

        if (response.ok) {
            const data = await response.json();
            // 4. Hiển thị câu trả lời từ Gemini
            addBotMessage(data.reply);
        } else {
            addBotMessage("Oa oa, PanPan bị mất kết nối rồi. Bạn thử lại sau nhé! 🐼💔");
        }
    } catch (error) {
        removeMessage(loadingId);
        console.error('Lỗi chat:', error);
        addBotMessage("Có chút trục trặc kỹ thuật nè. PanPan xin lỗi nha! 🥺");
    }
}
function addBotLoading() {
    const id = 'msg-' + Date.now();
    const div = document.createElement('div');
    div.className = 'message bot loading';
    div.id = id;
    div.innerHTML = '<span class="dot">.</span><span class="dot">.</span><span class="dot">.</span>'; // CSS animation cho dấu chấm
    chatBody.appendChild(div);
    scrollToBottom();
    return id;
}
function removeMessage(id) {
    const el = document.getElementById(id);
    if (el) el.remove();
}

function addUserMessage(text) {
    const div = document.createElement('div');
    div.className = 'message user';
    div.innerText = text;
    chatBody.appendChild(div);
    scrollToBottom();
}

function addBotMessage(html) {
    const div = document.createElement('div');
    div.className = 'message bot';
    div.innerHTML = html;
    chatBody.appendChild(div);
    scrollToBottom();
}

function scrollToBottom() {
    chatBody.scrollTop = chatBody.scrollHeight;
}

