import { Component, HostBinding, OnInit, inject } from '@angular/core';
import { LogService } from '../../../services/log.service';
import { DateFnsModule } from 'ngx-date-fns';

@Component({
  selector: 'app-file-history',
  standalone: true,
  imports: [DateFnsModule],
  templateUrl: './file-history.component.html',
  styleUrl: './file-history.component.css'
})
export class FileHistoryComponent implements OnInit {
  private logsService = inject(LogService);

  @HostBinding('class') get _classes(): string {
    return 'w-full';
  }

  logs: any[] = []

  ngOnInit(): void {
    this.logsService.getLogs()
      .subscribe(data => {
        data = data.map((h: any) => ({ ...h, createdAt: new Date(h.createdAt) })).reverse();
        this.logs = data;
      })
  }
}
