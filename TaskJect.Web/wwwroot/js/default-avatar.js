function getUserAvatarByName() {
    const avatars = document.querySelectorAll('.dynamic-avatar');

    avatars.forEach(avatar => {
        const name = avatar.getAttribute('data-name');

        if (!name || name.trim() === "") {
            return;
        }

        const firstLetter = name.charAt(0).toUpperCase();

        const imagePath = `/images/default-avatars/${firstLetter}.png`;

        avatar.src = imagePath;

        avatar.onerror = () => {
            avatar.src = "/images/default-avatars/default-avatar.png";
        };
    });
}
document.addEventListener('DOMContentLoaded', () => {
    getUserAvatarByName();
});
$(document).ajaxComplete(() => {
    getUserAvatarByName();
});