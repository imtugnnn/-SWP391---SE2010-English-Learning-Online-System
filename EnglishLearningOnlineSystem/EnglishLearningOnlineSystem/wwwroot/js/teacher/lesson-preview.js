(function () {
    "use strict";

    document.addEventListener("click", async function (event) {
        const trigger = event.target.closest("[data-lesson-preview-url]");
        if (!trigger) {
            return;
        }

        const drawerElement = document.getElementById("teacherLessonPreviewDrawer");
        const contentElement = document.getElementById("teacherLessonPreviewContent");
        if (!drawerElement || !contentElement) {
            return;
        }

        contentElement.innerHTML = '<div class="lesson-preview-state"><span class="spinner-border spinner-border-sm" aria-hidden="true"></span> Đang tải nội dung bài học...</div>';
        bootstrap.Offcanvas.getOrCreateInstance(drawerElement).show();

        try {
            const response = await fetch(trigger.dataset.lessonPreviewUrl, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (!response.ok) {
                throw new Error("Không thể tải nội dung bài học.");
            }

            contentElement.innerHTML = await response.text();
            updatePreviewSelectionAction(contentElement);
        } catch (error) {
            contentElement.innerHTML = '<div class="lesson-preview-state lesson-preview-state--error">Không thể tải nội dung bài học. Vui lòng thử lại.</div>';
        }
    });

    document.addEventListener("click", function (event) {
        const selectButton = event.target.closest("[data-preview-select-lesson]");
        if (!selectButton) {
            return;
        }

        const lessonId = selectButton.dataset.previewSelectLesson;
        const checkbox = document.querySelector(`.lesson-check[value="${CSS.escape(lessonId)}"]`);
        if (!checkbox) {
            return;
        }

        checkbox.checked = !checkbox.checked;
        checkbox.dispatchEvent(new Event("change", { bubbles: true }));
        updatePreviewSelectionAction(selectButton.closest(".lesson-preview"));
    });

    function updatePreviewSelectionAction(container) {
        const selectButton = container?.querySelector("[data-preview-select-lesson]");
        if (!selectButton) {
            return;
        }

        const lessonId = selectButton.dataset.previewSelectLesson;
        const checkbox = document.querySelector(`.lesson-check[value="${CSS.escape(lessonId)}"]`);
        if (!checkbox) {
            selectButton.hidden = true;
            return;
        }

        selectButton.hidden = false;
        selectButton.textContent = checkbox.checked ? "Bỏ chọn bài học" : "Chọn bài học này";
    }
})();
