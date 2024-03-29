﻿(function () {
    function messageToHtml(message, className) {
        return `<article class="message ${className}">
                    <header class="header">
                        <time class="date">${message.Timestamp}</time>
                        <h2>${message.UserName}</h2>
                    </header>
                    <main class="body">${message.Message}</main>
                </article>`;
    }
    function updateMessageList() {
        // Hämta användarid från den dolda input-taggen:
        const userId = $('#user-id').val();
        const recieverId = $('#reciever-id').val();
        $.get('/api/chatmessageapi/list')
            .then((resp) => {
                if (resp && Array.isArray(resp)) {
                    $('#messagelist').html('');
                    
                    resp.forEach((mess) => {
                        
                        if (mess.RecieverId === recieverId) {
                            const isMine = mess.UserId === userId;
                            $('#messagelist').append(
                                messageToHtml(mess, isMine ? 'own-message' : 'other-message')
                            );}
                        
                    });
                }
            });
    }

    function sendMessage() {
        const newMessage = $('#new-message').val();
        const timestamp = new Date().toISOString();
        const userId = $('#user-id').val();
        const recieverId = $('#reciever-id').val();
        

        if (newMessage) {
            const messageObj = {
                Message: newMessage,
                Timestamp: timestamp,
                UserId: userId,
                RecieverId: recieverId

            };
            $.post('/api/chatmessageapi/send', messageObj)
                .then((resp) => {
                    if (resp === "Meddelandet har skickats!") {
                        $('#new-message').val('');
                        updateMessageList();
                    } else {
                        alert("Något gick fel!");
                    }
                });
        }
    }

    window.addEventListener('load', () => {
        updateMessageList();

        if ($('#user-id').length > 0) {
            $('#send-button').click(sendMessage);
        }
    });
})();
