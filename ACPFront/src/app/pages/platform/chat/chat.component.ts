import { Component, ElementRef, HostBinding, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { ChatService } from '../../../services/chat.service';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';
import { DateFnsModule } from 'ngx-date-fns';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [ReactiveFormsModule, DateFnsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  private userService = inject(UserService);

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLElement>;

  @HostBinding('class') get _classes(): string {
    return 'w-full flex flex-col justify-between overflow-y-hidden';
  }

  messageFormGroup = new FormGroup({
    message: new FormControl('', [Validators.required])
  });

  usersList: any[] = [];
  chatMessages: any[] = [];

  intervalSubscription !: Subscription;

  shouldReset = true;
  currentSize = 0;

  ngOnInit(): void {
    this.intervalSubscription = interval(1000).subscribe(_ => {
      this.getMessages();
    });
  }

  ngOnDestroy(): void {
    this.intervalSubscription.unsubscribe();
  }

  getMessages(): void {
    this.userService.getUsers().subscribe(users => {
      this.usersList = users;

      this.chatService.getMessages().subscribe(messages => {
        this.chatMessages = messages.map((message: any) => {
          return {
            ...message,
            creationDate: new Date(message.creationDate)
          }
        });
        if (this.shouldReset || this.currentSize !== this.chatMessages.length) {
          setTimeout(() => {
            this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
          }, 0);
          document.querySelector('input')?.focus();
          this.shouldReset = false;
          this.currentSize = this.chatMessages.length;
        }
      })
    });
  }

  isCurrentUserMessage(message: any): boolean {
    return this.authService.getUserId() === message.ownerId;
  }

  onFormSubmission(): void {
    this.chatService.sendMessage(this.authService.getUserId(), this.messageFormGroup.value.message!).subscribe(_ => {
      this.messageFormGroup.reset();
      this.shouldReset = true;
      this.getMessages();
    })
  }

  getMessageOwnerFullName(message: any): string {
    const user = this.usersList.filter(user => user.id == message.ownerId)[0];
    return user?.fullName ?? 'User';
  }
}
